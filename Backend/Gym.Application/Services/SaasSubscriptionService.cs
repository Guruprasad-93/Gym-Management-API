using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Application.Payments;
using Gym.Domain.Constants;
using Microsoft.Extensions.Options;

namespace Gym.Application.Services;

public class TenantLimitService : ITenantLimitService
{
    private readonly ISaasSubscriptionRepository _repository;
    private readonly SaasSubscriptionSettings _settings;

    public TenantLimitService(
        ISaasSubscriptionRepository repository,
        IOptions<SaasSubscriptionSettings> saasSettings)
    {
        _repository = repository;
        _settings = saasSettings.Value;
    }

    public async Task EnsureHasAccessAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var check = await ResolveAccessCheckAsync(gymId, "Member", cancellationToken);
        if (check.HasAccess)
            return;

        if (string.IsNullOrWhiteSpace(check.PlanName))
            throw new InvalidOperationException("No active subscription found for this gym. Please subscribe to a plan to continue.");

        throw new InvalidOperationException("Your subscription has expired. Please upgrade your plan to continue.");
    }

    public async Task EnsureCanAddMemberAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var check = await ResolveAccessCheckAsync(gymId, "Member", cancellationToken);
        if (!check.HasAccess)
            await EnsureHasAccessAsync(gymId, cancellationToken);

        check = await _repository.CheckTenantLimitAsync(gymId, "Member", cancellationToken);
        if (check.MemberLimitReached)
            throw new InvalidOperationException(
                $"Member limit reached for {check.PlanName} plan ({check.CurrentMembers}/{check.MaxMembers}). Please upgrade your subscription.");
    }

    public async Task EnsureCanAddTrainerAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var check = await ResolveAccessCheckAsync(gymId, "Trainer", cancellationToken);
        if (!check.HasAccess)
            await EnsureHasAccessAsync(gymId, cancellationToken);

        check = await _repository.CheckTenantLimitAsync(gymId, "Trainer", cancellationToken);
        if (check.TrainerLimitReached)
            throw new InvalidOperationException(
                $"Trainer limit reached for {check.PlanName} plan ({check.CurrentTrainers}/{check.MaxTrainers}). Please upgrade your subscription.");
    }

    private async Task<TenantLimitCheckDto> ResolveAccessCheckAsync(
        Guid gymId,
        string resourceType,
        CancellationToken cancellationToken)
    {
        var check = await _repository.CheckTenantLimitAsync(gymId, resourceType, cancellationToken);
        if (check.HasAccess || !string.IsNullOrWhiteSpace(check.PlanName))
            return check;

        await _repository.CreateTrialSubscriptionAsync(gymId, _settings.GracePeriodDays, cancellationToken);
        return await _repository.CheckTenantLimitAsync(gymId, resourceType, cancellationToken);
    }
}

public class SaasSubscriptionService : ISaasSubscriptionService
{
    private readonly ISaasSubscriptionRepository _repository;
    private readonly IPlanManagementRepository _planRepository;
    private readonly IRazorpayGateway _razorpayGateway;
    private readonly ICurrentUserService _currentUser;
    private readonly ISubscriptionAccessService _subscriptionAccessService;
    private readonly RazorpaySettings _razorpaySettings;
    private readonly SaasSubscriptionSettings _saasSettings;

    public SaasSubscriptionService(
        ISaasSubscriptionRepository repository,
        IPlanManagementRepository planRepository,
        IRazorpayGateway razorpayGateway,
        ICurrentUserService currentUser,
        ISubscriptionAccessService subscriptionAccessService,
        IOptions<RazorpaySettings> razorpaySettings,
        IOptions<SaasSubscriptionSettings> saasSettings)
    {
        _repository = repository;
        _planRepository = planRepository;
        _razorpayGateway = razorpayGateway;
        _currentUser = currentUser;
        _subscriptionAccessService = subscriptionAccessService;
        _razorpaySettings = razorpaySettings.Value;
        _saasSettings = saasSettings.Value;
    }

    public Task<IReadOnlyList<SaasPlanDto>> GetPlansAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllPlansAsync(cancellationToken);

    public async Task<GymSubscriptionDto> GetCurrentSubscriptionAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        var subscription = await _repository.GetGymSubscriptionAsync(scope, cancellationToken)
            ?? throw new KeyNotFoundException("No subscription found for this gym.");
        return EnrichSubscription(subscription);
    }

    private GymSubscriptionDto EnrichSubscription(GymSubscriptionDto subscription)
    {
        var roles = _currentUser.Roles;
        if (roles.Count == 0)
            roles = new[] { RoleNames.GymAdmin };

        var access = _subscriptionAccessService.BuildState(subscription, roles);
        subscription.DaysToExpiry = access.DaysToExpiry;
        subscription.GraceDaysRemaining = access.GraceDaysRemaining;
        subscription.SubscriptionAccessMode = access.AccessMode;
        subscription.BannerMessage = access.BannerMessage;
        subscription.BannerSeverity = access.BannerSeverity;
        return subscription;
    }

    public async Task<GymUsageDto> GetUsageAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        return await _repository.GetGymUsageAsync(scope, cancellationToken);
    }

    public async Task<SaasPaymentOrderResponseDto> CreatePaymentOrderAsync(
        CreateSaasPaymentOrderDto dto, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        if (!_razorpaySettings.Enabled)
            throw new InvalidOperationException("Razorpay payments are not enabled.");

        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        var plan = await _repository.GetPlanByIdAsync(dto.SaasPlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan not found.");

        if (plan.IsTrialPlan)
            throw new InvalidOperationException("Cannot purchase the trial plan.");

        var subscription = await _repository.GetGymSubscriptionAsync(scope, cancellationToken)
            ?? throw new KeyNotFoundException("No subscription found.");

        string billingCycle;
        decimal amount;
        int? pricingOptionId = dto.PricingOptionId;

        if (pricingOptionId.HasValue)
        {
            var pricing = await _planRepository.GetPricingOptionByIdAsync(pricingOptionId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Pricing option not found.");

            if (pricing.SaasPlanId != plan.Id)
                throw new InvalidOperationException("Pricing option does not belong to the selected plan.");

            if (!pricing.IsActive)
                throw new InvalidOperationException("Pricing option is not active.");

            amount = pricing.Price;
            billingCycle = $"{pricing.DurationValue} {pricing.DurationUnit}";
        }
        else
        {
            billingCycle = SaasBillingCycleHelper.Normalize(dto.BillingCycle);
            amount = SaasBillingCycleHelper.ResolvePrice(plan, billingCycle);
        }

        var receipt = $"s-{scope.ToString("N")[..8]}-{plan.Id}-{DateTime.UtcNow:yyMMddHHmmss}";
        var orderId = await _razorpayGateway.CreateOrderAsync(amount, "INR", receipt,
            new Dictionary<string, string>
            {
                ["gymId"] = scope.ToString(),
                ["saasPlanId"] = plan.Id.ToString(),
                ["billingCycle"] = billingCycle,
                ["pricingOptionId"] = pricingOptionId?.ToString() ?? string.Empty
            }, cancellationToken);

        var paymentId = await _repository.CreatePendingPaymentAsync(
            scope, subscription.Id, plan.Id, amount, billingCycle, pricingOptionId, orderId, cancellationToken);

        var response = new SaasPaymentOrderResponseDto
        {
            SaasPaymentId = paymentId,
            RazorpayOrderId = orderId,
            Amount = amount,
            Currency = "INR",
            KeyId = _razorpaySettings.KeyId,
            PlanName = plan.PlanName,
            BillingCycle = billingCycle,
            PricingOptionId = pricingOptionId,
            DurationLabel = billingCycle
        };

        if (_razorpaySettings.UseMockGateway || RazorpayMockCheckoutHelper.IsMockOrder(orderId))
        {
            var mock = RazorpayMockCheckoutHelper.CreateSuccess(orderId, _razorpaySettings.KeySecret);
            response.UseMockCheckout = true;
            response.MockPaymentId = mock.PaymentId;
            response.MockSignature = mock.Signature;
        }

        return response;
    }

    public async Task<GymSubscriptionDto> VerifyPaymentAsync(
        VerifySaasPaymentDto dto, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        var pending = await _repository.GetPendingPaymentAsync(dto.SaasPaymentId, cancellationToken)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (pending.GymId != scope)
            throw new UnauthorizedAccessException("Payment does not belong to this gym.");

        if (!_razorpayGateway.VerifyPaymentSignature(dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature))
            throw new InvalidOperationException("Invalid payment signature.");

        if (string.Equals(pending.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            var existing = (await _repository.GetGymSubscriptionAsync(scope, cancellationToken))!;
            return EnrichSubscription(existing);
        }

        if (!string.Equals(pending.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Payment cannot be verified in status '{pending.Status}'.");

        var completion = await _repository.CompletePaymentAsync(dto.SaasPaymentId, dto.RazorpayPaymentId, cancellationToken);

        if (!completion.WasAlreadyCompleted)
        {
            await _repository.UpdateSubscriptionPlanAsync(
                scope, pending.SaasPlanId, pending.BillingCycle, pending.PricingOptionId, pending.Amount,
                dto.RazorpayOrderId, dto.RazorpayPaymentId, null, _saasSettings.GracePeriodDays, cancellationToken);
        }

        var updated = (await _repository.GetGymSubscriptionAsync(scope, cancellationToken))!;
        return EnrichSubscription(updated);
    }

    public async Task<GymSubscriptionDto> CancelAsync(bool cancelAtPeriodEnd = true, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        await _repository.CancelSubscriptionAsync(scope, cancelAtPeriodEnd, cancellationToken);
        return (await _repository.GetGymSubscriptionAsync(scope, cancellationToken))!;
    }

    public Task<SaasPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default) =>
        _repository.GetPlatformDashboardAsync(cancellationToken);
}

public class GymBrandingService : IGymBrandingService
{
    private readonly ISaasSubscriptionRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GymBrandingService(ISaasSubscriptionRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<GymBrandingDto> GetBrandingAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        return await _repository.GetGymBrandingAsync(scope, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");
    }

    public async Task<GymBrandingDto> UpdateBrandingAsync(UpdateGymBrandingDto dto, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        await _repository.UpdateGymBrandingAsync(scope, dto, cancellationToken);
        return (await _repository.GetGymBrandingAsync(scope, cancellationToken))!;
    }
}

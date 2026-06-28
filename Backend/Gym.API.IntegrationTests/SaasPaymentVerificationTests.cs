using Gym.Application.DTOs.Auth;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Application.Services;
using Gym.Domain.Constants;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gym.API.IntegrationTests;

public class SaasPaymentVerificationTests
{
    private static readonly Guid GymId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task VerifyPaymentAsync_PendingPayment_CompletesAndUpdatesSubscriptionOnce()
    {
        var repo = CreateRepository(paymentStatus: "Pending");
        var service = CreateService(repo, signatureValid: true);

        var result = await service.VerifyPaymentAsync(CreateVerifyDto(), GymId);

        Assert.Equal(1, repo.CompletePaymentCallCount);
        Assert.Equal(1, repo.UpdateSubscriptionPlanCallCount);
        Assert.Equal(GymId, result.GymId);
    }

    [Fact]
    public async Task VerifyPaymentAsync_AlreadyCompleted_SkipsSubscriptionUpdate()
    {
        var repo = CreateRepository(paymentStatus: "Completed");
        var service = CreateService(repo, signatureValid: true);

        await service.VerifyPaymentAsync(CreateVerifyDto(), GymId);

        Assert.Equal(0, repo.CompletePaymentCallCount);
        Assert.Equal(0, repo.UpdateSubscriptionPlanCallCount);
    }

    [Fact]
    public async Task VerifyPaymentAsync_AlreadyCompleted_ReturnsCurrentSubscription()
    {
        var repo = CreateRepository(paymentStatus: "Completed");
        repo.Subscription.PlanName = "Pro Annual";
        var service = CreateService(repo, signatureValid: true);

        var result = await service.VerifyPaymentAsync(CreateVerifyDto(), GymId);

        Assert.Equal("Pro Annual", result.PlanName);
    }

    [Fact]
    public async Task VerifyPaymentAsync_WasAlreadyCompletedFromSp_SkipsSubscriptionUpdate()
    {
        var repo = CreateRepository(paymentStatus: "Pending");
        repo.CompletionResult = new SaasPaymentCompletionResult { WasAlreadyCompleted = true };
        var service = CreateService(repo, signatureValid: true);

        await service.VerifyPaymentAsync(CreateVerifyDto(), GymId);

        Assert.Equal(1, repo.CompletePaymentCallCount);
        Assert.Equal(0, repo.UpdateSubscriptionPlanCallCount);
    }

    [Fact]
    public async Task VerifyPaymentAsync_DuplicateVerification_DoesNotDoubleExtendSubscription()
    {
        var repo = CreateRepository(paymentStatus: "Pending");
        var service = CreateService(repo, signatureValid: true);

        await service.VerifyPaymentAsync(CreateVerifyDto(), GymId);
        repo.PendingPayment!.Status = "Completed";

        await service.VerifyPaymentAsync(CreateVerifyDto(), GymId);

        Assert.Equal(1, repo.CompletePaymentCallCount);
        Assert.Equal(1, repo.UpdateSubscriptionPlanCallCount);
    }

    [Fact]
    public async Task VerifyPaymentAsync_InvalidSignature_ThrowsBeforeStateChange()
    {
        var repo = CreateRepository(paymentStatus: "Pending");
        var service = CreateService(repo, signatureValid: false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.VerifyPaymentAsync(CreateVerifyDto(), GymId));

        Assert.Equal(0, repo.CompletePaymentCallCount);
        Assert.Equal(0, repo.UpdateSubscriptionPlanCallCount);
    }

    [Fact]
    public async Task VerifyPaymentAsync_WrongGym_ThrowsBeforeStateChange()
    {
        var repo = CreateRepository(paymentStatus: "Pending");
        var service = CreateService(repo, signatureValid: true);
        var otherGym = Guid.Parse("22222222-2222-2222-2222-222222222222");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.VerifyPaymentAsync(CreateVerifyDto(), otherGym));

        Assert.Equal(0, repo.CompletePaymentCallCount);
        Assert.Equal(0, repo.UpdateSubscriptionPlanCallCount);
    }

    [Fact]
    public async Task VerifyPaymentAsync_FailedPayment_ThrowsBeforeStateChange()
    {
        var repo = CreateRepository(paymentStatus: "Failed");
        var service = CreateService(repo, signatureValid: true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.VerifyPaymentAsync(CreateVerifyDto(), GymId));

        Assert.Contains("Failed", ex.Message);
        Assert.Equal(0, repo.CompletePaymentCallCount);
        Assert.Equal(0, repo.UpdateSubscriptionPlanCallCount);
    }

    private static VerifySaasPaymentDto CreateVerifyDto() => new()
    {
        SaasPaymentId = 42,
        RazorpayOrderId = "order_test",
        RazorpayPaymentId = "pay_test",
        RazorpaySignature = "sig_test"
    };

    private static TestSaasSubscriptionRepository CreateRepository(string paymentStatus) =>
        new()
        {
            PendingPayment = new SaasPendingPaymentDto
            {
                SaasPaymentId = 42,
                GymId = GymId,
                GymSubscriptionId = 1,
                SaasPlanId = 2,
                Amount = 999m,
                BillingCycle = "Monthly",
                PricingOptionId = 5,
                RazorpayOrderId = "order_test",
                Status = paymentStatus,
                PlanName = "Starter"
            },
            Subscription = new GymSubscriptionDto
            {
                Id = 1,
                GymId = GymId,
                SaasPlanId = 2,
                PlanName = "Starter",
                Status = "Active",
                EndDate = new DateTime(2026, 8, 1)
            }
        };

    private static SaasSubscriptionService CreateService(TestSaasSubscriptionRepository repo, bool signatureValid)
    {
        var currentUser = new TestCurrentUserService(GymId);
        var access = new TestSubscriptionAccessService();
        return new SaasSubscriptionService(
            repo,
            new NotImplementedPlanRepository(),
            new TestRazorpayGateway(signatureValid),
            currentUser,
            access,
            Options.Create(new RazorpaySettings { Enabled = true, KeyId = "key_test" }),
            Options.Create(new SaasSubscriptionSettings { GracePeriodDays = 3 }));
    }

    private sealed class TestSaasSubscriptionRepository : ISaasSubscriptionRepository
    {
        public SaasPendingPaymentDto? PendingPayment { get; set; }
        public SaasPaymentCompletionResult CompletionResult { get; set; } = new() { WasAlreadyCompleted = false };
        public GymSubscriptionDto Subscription { get; set; } = new();
        public int CompletePaymentCallCount { get; private set; }
        public int UpdateSubscriptionPlanCallCount { get; private set; }

        public Task<SaasPendingPaymentDto?> GetPendingPaymentAsync(int saasPaymentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(PendingPayment);

        public Task<SaasPaymentCompletionResult> CompletePaymentAsync(int saasPaymentId, string razorpayPaymentId, CancellationToken cancellationToken = default)
        {
            CompletePaymentCallCount++;
            return Task.FromResult(CompletionResult);
        }

        public Task UpdateSubscriptionPlanAsync(
            Guid gymId, int saasPlanId, string? billingCycle, int? pricingOptionId, decimal amount,
            string? razorpayOrderId, string? razorpayPaymentId, string? razorpaySubscriptionId, int gracePeriodDays,
            CancellationToken cancellationToken = default)
        {
            UpdateSubscriptionPlanCallCount++;
            return Task.CompletedTask;
        }

        public Task<GymSubscriptionDto?> GetGymSubscriptionAsync(Guid gymId, CancellationToken cancellationToken = default) =>
            Task.FromResult<GymSubscriptionDto?>(Subscription);

        public Task<IReadOnlyList<SaasPlanDto>> GetAllPlansAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<SaasPlanDto?> GetPlanByIdAsync(int saasPlanId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<GymUsageDto> GetGymUsageAsync(Guid gymId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<TenantLimitCheckDto> CheckTenantLimitAsync(Guid gymId, string resourceType, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<int> CreateTrialSubscriptionAsync(Guid gymId, int gracePeriodDays, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task CancelSubscriptionAsync(Guid gymId, bool cancelAtPeriodEnd, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<int> CreatePendingPaymentAsync(Guid gymId, int gymSubscriptionId, int saasPlanId, decimal amount, string? billingCycle, int? pricingOptionId, string razorpayOrderId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<SaasPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task ExpireSubscriptionsAsync(int gracePeriodDays, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task SeedNotificationSettingsAsync(Guid gymId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpdateGymBrandingAsync(Guid gymId, UpdateGymBrandingDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<GymBrandingDto?> GetGymBrandingAsync(Guid gymId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class TestRazorpayGateway(bool valid) : IRazorpayGateway
    {
        public Task<string> CreateOrderAsync(decimal amountInRupees, string currency, string receipt, IReadOnlyDictionary<string, string>? notes, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature) => valid;

        public Task<string> RefundPaymentAsync(string razorpayPaymentId, decimal? amountInRupees, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class TestCurrentUserService(Guid gymId) : ICurrentUserService
    {
        public Guid? UserId => Guid.NewGuid();
        public Guid? GymId => gymId;
        public bool IsAuthenticated => true;
        public IReadOnlyList<string> Permissions { get; } = Array.Empty<string>();
        public IReadOnlyList<string> Roles { get; } = new[] { RoleNames.GymAdmin };

        public bool HasPermission(string permission) => false;
        public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        public Guid RequireGymId() => gymId;
    }

    private sealed class TestSubscriptionAccessService : ISubscriptionAccessService
    {
        public Task<SubscriptionAccessStateDto> ResolveAsync(Guid gymId, IReadOnlyList<string> roles, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SubscriptionAccessStateDto());

        public SubscriptionAccessStateDto BuildState(GymSubscriptionDto subscription, IReadOnlyList<string> roles) =>
            new() { DaysToExpiry = 30 };
    }

    private sealed class NotImplementedPlanRepository : IPlanManagementRepository
    {
        public Task<IReadOnlyList<PlanSummaryDto>> GetPlatformPlanSummariesAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<SaasPlanDto>> GetPlatformPlansAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<DynamicSaasPlanDto?> GetPlanDetailAsync(int saasPlanId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<int> CreatePlanAsync(CreateDynamicPlanDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<int> ClonePlanAsync(int sourceSaasPlanId, CloneDynamicPlanDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpdatePlanAsync(int saasPlanId, UpdateDynamicPlanDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task DeletePlanAsync(int saasPlanId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpsertQuotasAsync(int saasPlanId, UpsertPlanQuotaDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task SetPlanFeaturesAsync(int saasPlanId, IReadOnlyList<int> featureIds, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<int> CreatePricingOptionAsync(int saasPlanId, CreatePlanPricingOptionDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpdatePricingOptionAsync(int pricingOptionId, UpdatePlanPricingOptionDto dto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task DeletePricingOptionAsync(int pricingOptionId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task ReorderPricingOptionsAsync(int saasPlanId, IReadOnlyList<PricingOptionOrderDto> items, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<PlanPricingOptionDto?> GetPricingOptionByIdAsync(int pricingOptionId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<SaasPlanCatalogDto> GetPlanCatalogAsync(bool publicOnly = true, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}

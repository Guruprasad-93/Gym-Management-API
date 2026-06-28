using Gym.Application.DTOs.Saas;

namespace Gym.Application.Interfaces;

public interface ISaasSubscriptionRepository
{
    Task<IReadOnlyList<SaasPlanDto>> GetAllPlansAsync(CancellationToken cancellationToken = default);
    Task<SaasPlanDto?> GetPlanByIdAsync(int saasPlanId, CancellationToken cancellationToken = default);
    Task<GymSubscriptionDto?> GetGymSubscriptionAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<GymUsageDto> GetGymUsageAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<TenantLimitCheckDto> CheckTenantLimitAsync(Guid gymId, string resourceType, CancellationToken cancellationToken = default);
    Task<int> CreateTrialSubscriptionAsync(Guid gymId, int gracePeriodDays, CancellationToken cancellationToken = default);
    Task UpdateSubscriptionPlanAsync(Guid gymId, int saasPlanId, string? billingCycle, int? pricingOptionId, decimal amount,
        string? razorpayOrderId, string? razorpayPaymentId, string? razorpaySubscriptionId, int gracePeriodDays,
        CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(Guid gymId, bool cancelAtPeriodEnd, CancellationToken cancellationToken = default);
    Task<int> CreatePendingPaymentAsync(Guid gymId, int gymSubscriptionId, int saasPlanId, decimal amount,
        string? billingCycle, int? pricingOptionId, string razorpayOrderId, CancellationToken cancellationToken = default);
    Task<SaasPaymentCompletionResult> CompletePaymentAsync(int saasPaymentId, string razorpayPaymentId, CancellationToken cancellationToken = default);
    Task<SaasPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default);
    Task ExpireSubscriptionsAsync(int gracePeriodDays, CancellationToken cancellationToken = default);
    Task SeedNotificationSettingsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task UpdateGymBrandingAsync(Guid gymId, UpdateGymBrandingDto dto, CancellationToken cancellationToken = default);
    Task<GymBrandingDto?> GetGymBrandingAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<SaasPendingPaymentDto?> GetPendingPaymentAsync(int saasPaymentId, CancellationToken cancellationToken = default);
}

public interface IGymOnboardingService
{
    Task<RegisterGymResultDto> RegisterGymAsync(RegisterGymDto dto, CancellationToken cancellationToken = default);
}

public interface ISaasSubscriptionService
{
    Task<IReadOnlyList<SaasPlanDto>> GetPlansAsync(CancellationToken cancellationToken = default);
    Task<GymSubscriptionDto> GetCurrentSubscriptionAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<GymUsageDto> GetUsageAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<SaasPaymentOrderResponseDto> CreatePaymentOrderAsync(CreateSaasPaymentOrderDto dto, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<GymSubscriptionDto> VerifyPaymentAsync(VerifySaasPaymentDto dto, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<GymSubscriptionDto> CancelAsync(bool cancelAtPeriodEnd = true, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<SaasPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default);
}

public interface ITenantLimitService
{
    Task EnsureCanAddMemberAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task EnsureCanAddTrainerAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task EnsureHasAccessAsync(Guid gymId, CancellationToken cancellationToken = default);
}

public interface IGymBrandingService
{
    Task<GymBrandingDto> GetBrandingAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<GymBrandingDto> UpdateBrandingAsync(UpdateGymBrandingDto dto, Guid? gymId = null, CancellationToken cancellationToken = default);
}

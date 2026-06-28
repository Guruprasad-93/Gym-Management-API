using System.Data;
using Dapper;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class SaasSubscriptionRepository : ISaasSubscriptionRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public SaasSubscriptionRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<IReadOnlyList<SaasPlanDto>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<SaasPlanRow>(StoredProcedureNames.SaasGetAllPlans, cancellationToken: cancellationToken);
        return rows.Select(MapPlan).ToList();
    }

    public async Task<SaasPlanDto?> GetPlanByIdAsync(int saasPlanId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<SaasPlanRow>(
            StoredProcedureNames.SaasGetPlanById, new { SaasPlanId = saasPlanId }, cancellationToken);
        return row is null ? null : MapPlan(row);
    }

    public async Task<GymSubscriptionDto?> GetGymSubscriptionAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<GymSubscriptionRow>(
            StoredProcedureNames.SaasGetGymSubscription, new { GymId = gymId }, cancellationToken);
        return row is null ? null : MapSubscription(row);
    }

    public async Task<GymUsageDto> GetGymUsageAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<GymUsageRow>(
            StoredProcedureNames.SaasGetGymUsage, new { GymId = gymId }, cancellationToken);
        return new GymUsageDto
        {
            MemberCount = row?.MemberCount ?? 0,
            TrainerCount = row?.TrainerCount ?? 0,
            StorageUsedBytes = row?.StorageUsedBytes ?? 0,
            WhatsAppSentThisMonth = row?.WhatsAppSentThisMonth ?? 0
        };
    }

    public async Task<TenantLimitCheckDto> CheckTenantLimitAsync(Guid gymId, string resourceType, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<TenantLimitRow>(
            StoredProcedureNames.SaasCheckTenantLimit, new { GymId = gymId, ResourceType = resourceType }, cancellationToken);
        return new TenantLimitCheckDto
        {
            HasAccess = row?.HasAccess ?? false,
            PlanName = row?.PlanName ?? string.Empty,
            MaxMembers = row?.MaxMembers ?? 0,
            MaxTrainers = row?.MaxTrainers ?? 0,
            CurrentMembers = row?.CurrentMembers ?? 0,
            CurrentTrainers = row?.CurrentTrainers ?? 0,
            MemberLimitReached = row?.MemberLimitReached ?? false,
            TrainerLimitReached = row?.TrainerLimitReached ?? false
        };
    }

    public async Task<int> CreateTrialSubscriptionAsync(Guid gymId, int gracePeriodDays, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@GracePeriodDays", gracePeriodDays);
        parameters.Add("@GymSubscriptionId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.SaasCreateTrialSubscription, parameters, cancellationToken);
        return parameters.Get<int>("@GymSubscriptionId");
    }

    public Task UpdateSubscriptionPlanAsync(Guid gymId, int saasPlanId, string? billingCycle, int? pricingOptionId, decimal amount,
        string? razorpayOrderId, string? razorpayPaymentId, string? razorpaySubscriptionId, int gracePeriodDays,
        CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SaasUpdateSubscriptionPlan, new
        {
            GymId = gymId,
            SaasPlanId = saasPlanId,
            BillingCycle = billingCycle,
            PricingOptionId = pricingOptionId,
            Amount = amount,
            RazorpayOrderId = razorpayOrderId,
            RazorpayPaymentId = razorpayPaymentId,
            RazorpaySubscriptionId = razorpaySubscriptionId,
            GracePeriodDays = gracePeriodDays
        }, cancellationToken);

    public Task CancelSubscriptionAsync(Guid gymId, bool cancelAtPeriodEnd, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SaasCancelSubscription,
            new { GymId = gymId, CancelAtPeriodEnd = cancelAtPeriodEnd }, cancellationToken);

    public async Task<int> CreatePendingPaymentAsync(Guid gymId, int gymSubscriptionId, int saasPlanId, decimal amount,
        string? billingCycle, int? pricingOptionId, string razorpayOrderId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@GymSubscriptionId", gymSubscriptionId);
        parameters.Add("@SaasPlanId", saasPlanId);
        parameters.Add("@Amount", amount);
        parameters.Add("@BillingCycle", billingCycle);
        parameters.Add("@PricingOptionId", pricingOptionId);
        parameters.Add("@RazorpayOrderId", razorpayOrderId);
        parameters.Add("@SaasPaymentId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.SaasCreatePendingPayment, parameters, cancellationToken);
        return parameters.Get<int>("@SaasPaymentId");
    }

    public async Task<SaasPaymentCompletionResult> CompletePaymentAsync(int saasPaymentId, string razorpayPaymentId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@SaasPaymentId", saasPaymentId);
        parameters.Add("@RazorpayPaymentId", razorpayPaymentId);
        parameters.Add("@WasAlreadyCompleted", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.SaasCompletePayment, parameters, cancellationToken);
        return new SaasPaymentCompletionResult
        {
            WasAlreadyCompleted = parameters.Get<bool>("@WasAlreadyCompleted")
        };
    }

    public async Task<SaasPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<SaasPlatformDashboardRow>(
            StoredProcedureNames.SaasGetPlatformDashboard, cancellationToken: cancellationToken);
        return new SaasPlatformDashboardDto
        {
            TotalGyms = row?.TotalGyms ?? 0,
            ActiveGyms = row?.ActiveGyms ?? 0,
            ActiveSubscriptions = row?.ActiveSubscriptions ?? 0,
            ExpiredSubscriptions = row?.ExpiredSubscriptions ?? 0,
            TrialSubscriptions = row?.TrialSubscriptions ?? 0,
            MonthlyRecurringRevenue = row?.MonthlyRecurringRevenue ?? 0,
            AnnualRecurringRevenue = row?.AnnualRecurringRevenue ?? 0
        };
    }

    public Task ExpireSubscriptionsAsync(int gracePeriodDays, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SaasExpireSubscriptions,
            new { GracePeriodDays = gracePeriodDays }, cancellationToken);

    public Task SeedNotificationSettingsAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SaasSeedNotificationSettings, new { GymId = gymId }, cancellationToken);

    public Task UpdateGymBrandingAsync(Guid gymId, UpdateGymBrandingDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.GymUpdateBranding, new
        {
            GymId = gymId,
            dto.PrimaryColor,
            dto.SecondaryColor,
            dto.BannerFileId,
            dto.ReceiptHeaderText,
            dto.InvoiceFooterText
        }, cancellationToken);

    public async Task<GymBrandingDto?> GetGymBrandingAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<GymRow>(
            StoredProcedureNames.GetGymById, new { GymId = gymId }, cancellationToken);
        if (row is null) return null;
        return new GymBrandingDto
        {
            GymId = row.GymId,
            LogoUrl = row.LogoUrl,
            PrimaryColor = row.PrimaryColor,
            SecondaryColor = row.SecondaryColor,
            BannerFileId = row.BannerFileId,
            ReceiptHeaderText = row.ReceiptHeaderText,
            InvoiceFooterText = row.InvoiceFooterText
        };
    }

    public async Task<SaasPendingPaymentDto?> GetPendingPaymentAsync(int saasPaymentId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<SaasPendingPaymentRow>(
            StoredProcedureNames.SaasGetPendingPayment, new { SaasPaymentId = saasPaymentId }, cancellationToken);
        return row is null ? null : new SaasPendingPaymentDto
        {
            SaasPaymentId = row.SaasPaymentId,
            GymId = row.GymId,
            GymSubscriptionId = row.GymSubscriptionId,
            SaasPlanId = row.SaasPlanId,
            Amount = row.Amount,
            BillingCycle = row.BillingCycle,
            PricingOptionId = row.PricingOptionId,
            RazorpayOrderId = row.RazorpayOrderId,
            Status = row.Status,
            PlanName = row.PlanName
        };
    }

    private static SaasPlanDto MapPlan(SaasPlanRow row) => new()
    {
        Id = row.SaasPlanId,
        PlanCode = row.PlanCode,
        PlanName = row.PlanName,
        Description = row.Description,
        IsTrialPlan = row.IsTrialPlan,
        IsPublic = row.IsPublic,
        MaxMembers = row.MaxMembers,
        MaxTrainers = row.MaxTrainers,
        MaxBranches = row.MaxBranches,
        MaxStorageGB = row.MaxStorageGB,
        MaxSmsPerMonth = row.MaxSmsPerMonth,
        MaxWhatsappMessages = row.MaxWhatsappMessages,
        StorageLimitMb = row.StorageLimitMb,
        WhatsAppNotificationLimit = row.WhatsAppNotificationLimit,
        MonthlyPrice = row.MonthlyPrice,
        QuarterlyPrice = row.QuarterlyPrice,
        HalfYearlyPrice = row.HalfYearlyPrice,
        YearlyPrice = row.YearlyPrice,
        TrialDays = row.TrialDays,
        IsActive = row.IsActive,
        SortOrder = row.SortOrder
    };

    private static GymSubscriptionDto MapSubscription(GymSubscriptionRow row) => new()
    {
        Id = row.GymSubscriptionId,
        GymId = row.GymId,
        SaasPlanId = row.SaasPlanId,
        PlanCode = row.PlanCode,
        PlanName = row.PlanName,
        Status = row.Status,
        BillingCycle = row.BillingCycle,
        PricingOptionId = row.PricingOptionId,
        DurationValue = row.DurationValue,
        DurationUnit = row.DurationUnit,
        Amount = row.Amount,
        StartDate = row.StartDate,
        EndDate = row.EndDate,
        TrialEndsAt = row.TrialEndsAt,
        CurrentPeriodEnd = row.CurrentPeriodEnd,
        GraceEndsAt = row.GraceEndsAt,
        RemainingTrialDays = row.RemainingTrialDays,
        HasAccess = row.HasAccess,
        CancelAtPeriodEnd = row.CancelAtPeriodEnd,
        MaxMembers = row.MaxMembers,
        MaxTrainers = row.MaxTrainers,
        StorageLimitMb = row.StorageLimitMb,
        WhatsAppNotificationLimit = row.WhatsAppNotificationLimit,
        MonthlyPrice = row.MonthlyPrice,
        YearlyPrice = row.YearlyPrice
    };
}

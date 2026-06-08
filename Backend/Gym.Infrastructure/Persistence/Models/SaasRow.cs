namespace Gym.Infrastructure.Persistence.Models;

internal sealed class SaasPlanRow
{
    public int SaasPlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int StorageLimitMb { get; set; }
    public int WhatsAppNotificationLimit { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public int TrialDays { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

internal sealed class GymSubscriptionRow
{
    public int GymSubscriptionId { get; set; }
    public Guid GymId { get; set; }
    public int SaasPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? BillingCycle { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? GraceEndsAt { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySubscriptionId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int StorageLimitMb { get; set; }
    public int WhatsAppNotificationLimit { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public int? RemainingTrialDays { get; set; }
    public bool HasAccess { get; set; }
}

internal sealed class GymUsageRow
{
    public int MemberCount { get; set; }
    public int TrainerCount { get; set; }
    public long StorageUsedBytes { get; set; }
    public int WhatsAppSentThisMonth { get; set; }
}

internal sealed class TenantLimitRow
{
    public bool HasAccess { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int CurrentMembers { get; set; }
    public int CurrentTrainers { get; set; }
    public bool MemberLimitReached { get; set; }
    public bool TrainerLimitReached { get; set; }
}

internal sealed class SaasPlatformDashboardRow
{
    public int TotalGyms { get; set; }
    public int ActiveGyms { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal AnnualRecurringRevenue { get; set; }
}

internal sealed class SaasPendingPaymentRow
{
    public int SaasPaymentId { get; set; }
    public Guid GymId { get; set; }
    public int GymSubscriptionId { get; set; }
    public int SaasPlanId { get; set; }
    public decimal Amount { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public string? RazorpayOrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
}

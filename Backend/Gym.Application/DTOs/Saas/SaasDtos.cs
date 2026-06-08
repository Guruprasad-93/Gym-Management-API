namespace Gym.Application.DTOs.Saas;

public class SaasPlanDto
{
    public int Id { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int StorageLimitMb { get; set; }
    public int WhatsAppNotificationLimit { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public int TrialDays { get; set; }
}

public class GymSubscriptionDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int SaasPlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? BillingCycle { get; set; }
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? GraceEndsAt { get; set; }
    public int? RemainingTrialDays { get; set; }
    public bool HasAccess { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int StorageLimitMb { get; set; }
    public int WhatsAppNotificationLimit { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
}

public class GymUsageDto
{
    public int MemberCount { get; set; }
    public int TrainerCount { get; set; }
    public long StorageUsedBytes { get; set; }
    public int WhatsAppSentThisMonth { get; set; }
}

public class TenantLimitCheckDto
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

public class RegisterGymDto
{
    public string GymName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Password { get; set; }
}

public class RegisterGymResultDto
{
    public Guid GymId { get; set; }
    public Guid AdminUserId { get; set; }
    public string GymName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string? TemporaryPassword { get; set; }
    public int RemainingTrialDays { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ChangeSaasPlanDto
{
    public int SaasPlanId { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
}

public class CreateSaasPaymentOrderDto
{
    public int SaasPlanId { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
}

public class SaasPaymentOrderResponseDto
{
    public int SaasPaymentId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string KeyId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
}

public class VerifySaasPaymentDto
{
    public int SaasPaymentId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}

public class SaasPlatformDashboardDto
{
    public int TotalGyms { get; set; }
    public int ActiveGyms { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal AnnualRecurringRevenue { get; set; }
}

public class UpdateGymBrandingDto
{
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public long? BannerFileId { get; set; }
    public string? ReceiptHeaderText { get; set; }
    public string? InvoiceFooterText { get; set; }
}

public class GymBrandingDto
{
    public Guid GymId { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public long? BannerFileId { get; set; }
    public string? BannerUrl { get; set; }
    public string? ReceiptHeaderText { get; set; }
    public string? InvoiceFooterText { get; set; }
}

public class SaasPendingPaymentDto
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

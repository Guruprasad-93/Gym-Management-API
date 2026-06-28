namespace Gym.Infrastructure.Persistence.Models;

internal sealed class SystemFeatureRow
{
    public int FeatureId { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? MenuRoute { get; set; }
    public string? MenuIcon { get; set; }
    public bool IsMenuFeature { get; set; }
    public bool IsApiFeature { get; set; }
    public bool IsQuotaFeature { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

internal sealed class FeatureApiRouteRow
{
    public string FeatureCode { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = string.Empty;
    public string? HttpMethods { get; set; }
}

internal sealed class PlanQuotaRow
{
    public int PlanQuotaId { get; set; }
    public int SaasPlanId { get; set; }
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int MaxBranches { get; set; }
    public int MaxStorageGB { get; set; }
    public int MaxSmsPerMonth { get; set; }
    public int MaxWhatsappMessages { get; set; }
    public int StorageLimitMb { get; set; }
    public int WhatsAppNotificationLimit { get; set; }
}

internal sealed class PlanFeatureRow
{
    public int PlanFeatureId { get; set; }
    public int SaasPlanId { get; set; }
    public int FeatureId { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsIncluded { get; set; }
}

internal sealed class PlanPricingRow
{
    public int PricingOptionId { get; set; }
    public int SaasPlanId { get; set; }
    public int DurationValue { get; set; }
    public string DurationUnit { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "INR";
    public string? DisplayLabel { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

internal sealed class SaasPlanDetailRow
{
    public int SaasPlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTrialPlan { get; set; }
    public bool IsPublic { get; set; }
    public int TrialDays { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal QuarterlyPrice { get; set; }
    public decimal HalfYearlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
}

internal sealed class SaasCatalogPlanRow
{
    public int SaasPlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int MaxBranches { get; set; }
    public int MaxStorageGB { get; set; }
    public int MaxSmsPerMonth { get; set; }
    public int MaxWhatsappMessages { get; set; }
}

internal sealed class PlanSummaryRow
{
    public int SaasPlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTrialPlan { get; set; }
    public bool IsPublic { get; set; }
    public int TrialDays { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ActiveSubscriberCount { get; set; }
    public int FeatureCount { get; set; }
    public int PricingOptionCount { get; set; }
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int MaxBranches { get; set; }
    public int MaxStorageGB { get; set; }
    public int MaxSmsPerMonth { get; set; }
    public int MaxWhatsappMessages { get; set; }
}

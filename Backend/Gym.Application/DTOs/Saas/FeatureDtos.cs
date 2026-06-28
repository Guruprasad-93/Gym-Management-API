namespace Gym.Application.DTOs.Saas;

public class SystemFeatureDto
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

public class PlanPricingOptionDto
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

public class PlanQuotaDto
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

public class PlanFeatureAssignmentDto
{
    public int PlanFeatureId { get; set; }
    public int SaasPlanId { get; set; }
    public int FeatureId { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsIncluded { get; set; }
}

public class PlanSummaryDto
{
    public int Id { get; set; }
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
    public PlanQuotaDto? Quotas { get; set; }
}

public class DynamicSaasPlanDto
{
    public int Id { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTrialPlan { get; set; }
    public bool IsPublic { get; set; }
    public int TrialDays { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int ActiveSubscriberCount { get; set; }
    public int FeatureCount { get; set; }
    public int PricingOptionCount { get; set; }
    public PlanQuotaDto? Quotas { get; set; }
    public IReadOnlyList<PlanFeatureAssignmentDto> Features { get; set; } = Array.Empty<PlanFeatureAssignmentDto>();
    public IReadOnlyList<PlanPricingOptionDto> PricingOptions { get; set; } = Array.Empty<PlanPricingOptionDto>();
    public decimal MonthlyPrice { get; set; }
    public decimal QuarterlyPrice { get; set; }
    public decimal HalfYearlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
}

public class SaasPlanCatalogDto
{
    public IReadOnlyList<SaasPlanCatalogItemDto> Plans { get; set; } = Array.Empty<SaasPlanCatalogItemDto>();
}

public class SaasPlanCatalogItemDto
{
    public int Id { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public PlanQuotaDto Quotas { get; set; } = new();
    public IReadOnlyList<PlanPricingOptionDto> PricingOptions { get; set; } = Array.Empty<PlanPricingOptionDto>();
    public IReadOnlyList<PlanFeatureAssignmentDto> Features { get; set; } = Array.Empty<PlanFeatureAssignmentDto>();
}

public class GymFeaturesDto
{
    public IReadOnlyList<string> SubscriptionFeatureCodes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> EnabledFeatureCodes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> VisibleMenuCodes { get; set; } = Array.Empty<string>();
}

public class CreateDynamicPlanDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTrialPlan { get; set; }
    public bool IsPublic { get; set; } = true;
    public int TrialDays { get; set; }
    public int SortOrder { get; set; }
    public UpsertPlanQuotaDto? Quotas { get; set; }
    public IReadOnlyList<int> FeatureIds { get; set; } = Array.Empty<int>();
}

public class UpdateDynamicPlanDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsTrialPlan { get; set; }
    public bool IsPublic { get; set; } = true;
    public int TrialDays { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public UpsertPlanQuotaDto? Quotas { get; set; }
    public IReadOnlyList<int> FeatureIds { get; set; } = Array.Empty<int>();
}

public class UpsertPlanQuotaDto
{
    public int MaxMembers { get; set; }
    public int MaxTrainers { get; set; }
    public int MaxBranches { get; set; } = 1;
    public int MaxStorageGB { get; set; }
    public int MaxSmsPerMonth { get; set; }
    public int MaxWhatsappMessages { get; set; }
}

public class CreatePlanPricingOptionDto
{
    public int DurationValue { get; set; }
    public string DurationUnit { get; set; } = "Month";
    public decimal Price { get; set; }
    public string? DisplayLabel { get; set; }
    public int SortOrder { get; set; }
}

public class UpdatePlanPricingOptionDto
{
    public int DurationValue { get; set; }
    public string DurationUnit { get; set; } = "Month";
    public decimal Price { get; set; }
    public string? DisplayLabel { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class FeatureApiRouteDto
{
    public string FeatureCode { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = string.Empty;
    public string? HttpMethods { get; set; }
}

public class FeatureDependencyDto
{
    public string FeatureCode { get; set; } = string.Empty;
    public string RequiresFeatureCode { get; set; } = string.Empty;
}

public class CloneDynamicPlanDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsTrialPlan { get; set; }
    public bool? IsPublic { get; set; }
    public int? SortOrder { get; set; }
}

public class ReorderPlanPricingOptionsDto
{
    public IReadOnlyList<PricingOptionOrderDto> Items { get; set; } = Array.Empty<PricingOptionOrderDto>();
}

public class PricingOptionOrderDto
{
    public int PricingOptionId { get; set; }
    public int SortOrder { get; set; }
}

public class ValidatePlanFeaturesDto
{
    public IReadOnlyList<int> FeatureIds { get; set; } = Array.Empty<int>();
}

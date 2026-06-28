using Gym.Application.Authorization;
using Gym.Application.DTOs.Saas;

namespace Gym.Application.Interfaces;

public interface IPlanManagementService
{
    Task<IReadOnlyList<SystemFeatureDto>> GetFeaturesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanSummaryDto>> GetPlatformPlanSummariesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DynamicSaasPlanDto>> GetPlatformPlansAsync(CancellationToken cancellationToken = default);
    Task<DynamicSaasPlanDto> GetPlanDetailAsync(int saasPlanId, CancellationToken cancellationToken = default);
    Task<DynamicSaasPlanDto> CreatePlanAsync(CreateDynamicPlanDto dto, CancellationToken cancellationToken = default);
    Task<DynamicSaasPlanDto> UpdatePlanAsync(int saasPlanId, UpdateDynamicPlanDto dto, CancellationToken cancellationToken = default);
    Task<DynamicSaasPlanDto> ClonePlanAsync(int sourceSaasPlanId, CloneDynamicPlanDto dto, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(int saasPlanId, CancellationToken cancellationToken = default);
    Task<PlanPricingOptionDto> CreatePricingOptionAsync(int saasPlanId, CreatePlanPricingOptionDto dto, CancellationToken cancellationToken = default);
    Task<PlanPricingOptionDto> UpdatePricingOptionAsync(int pricingOptionId, UpdatePlanPricingOptionDto dto, CancellationToken cancellationToken = default);
    Task DeletePricingOptionAsync(int pricingOptionId, CancellationToken cancellationToken = default);
    Task ReorderPricingOptionsAsync(int saasPlanId, ReorderPlanPricingOptionsDto dto, CancellationToken cancellationToken = default);
    Task<FeatureDependencyValidationResult> ValidateFeaturesAsync(ValidatePlanFeaturesDto dto, CancellationToken cancellationToken = default);
    Task<SaasPlanCatalogDto> GetPlanCatalogAsync(CancellationToken cancellationToken = default);
    Task<GymFeaturesDto> GetMyFeaturesAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
}

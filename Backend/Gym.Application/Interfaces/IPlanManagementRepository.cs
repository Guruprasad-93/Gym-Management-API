using Gym.Application.DTOs.Saas;

namespace Gym.Application.Interfaces;

public interface IPlanManagementRepository
{
    Task<IReadOnlyList<PlanSummaryDto>> GetPlatformPlanSummariesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaasPlanDto>> GetPlatformPlansAsync(CancellationToken cancellationToken = default);
    Task<DynamicSaasPlanDto?> GetPlanDetailAsync(int saasPlanId, CancellationToken cancellationToken = default);
    Task<int> CreatePlanAsync(CreateDynamicPlanDto dto, CancellationToken cancellationToken = default);
    Task<int> ClonePlanAsync(int sourceSaasPlanId, CloneDynamicPlanDto dto, CancellationToken cancellationToken = default);
    Task UpdatePlanAsync(int saasPlanId, UpdateDynamicPlanDto dto, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(int saasPlanId, CancellationToken cancellationToken = default);
    Task UpsertQuotasAsync(int saasPlanId, UpsertPlanQuotaDto dto, CancellationToken cancellationToken = default);
    Task SetPlanFeaturesAsync(int saasPlanId, IReadOnlyList<int> featureIds, CancellationToken cancellationToken = default);
    Task<int> CreatePricingOptionAsync(int saasPlanId, CreatePlanPricingOptionDto dto, CancellationToken cancellationToken = default);
    Task UpdatePricingOptionAsync(int pricingOptionId, UpdatePlanPricingOptionDto dto, CancellationToken cancellationToken = default);
    Task DeletePricingOptionAsync(int pricingOptionId, CancellationToken cancellationToken = default);
    Task ReorderPricingOptionsAsync(int saasPlanId, IReadOnlyList<PricingOptionOrderDto> items, CancellationToken cancellationToken = default);
    Task<PlanPricingOptionDto?> GetPricingOptionByIdAsync(int pricingOptionId, CancellationToken cancellationToken = default);
    Task<SaasPlanCatalogDto> GetPlanCatalogAsync(bool publicOnly = true, CancellationToken cancellationToken = default);
}

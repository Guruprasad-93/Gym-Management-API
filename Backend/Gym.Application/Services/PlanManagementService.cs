using Gym.Application.Authorization;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class PlanManagementService : IPlanManagementService
{
    private readonly IPlanManagementRepository _planRepository;
    private readonly IFeatureRepository _featureRepository;
    private readonly IFeatureResolverService _featureResolver;
    private readonly IFeatureDependencyService _featureDependencyService;
    private readonly ICurrentUserService _currentUser;

    public PlanManagementService(
        IPlanManagementRepository planRepository,
        IFeatureRepository featureRepository,
        IFeatureResolverService featureResolver,
        IFeatureDependencyService featureDependencyService,
        ICurrentUserService currentUser)
    {
        _planRepository = planRepository;
        _featureRepository = featureRepository;
        _featureResolver = featureResolver;
        _featureDependencyService = featureDependencyService;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<SystemFeatureDto>> GetFeaturesAsync(CancellationToken cancellationToken = default) =>
        _featureRepository.GetAllFeaturesAsync(cancellationToken: cancellationToken);

    public Task<IReadOnlyList<PlanSummaryDto>> GetPlatformPlanSummariesAsync(CancellationToken cancellationToken = default) =>
        _planRepository.GetPlatformPlanSummariesAsync(cancellationToken);

    public async Task<IReadOnlyList<DynamicSaasPlanDto>> GetPlatformPlansAsync(CancellationToken cancellationToken = default)
    {
        var summaries = await _planRepository.GetPlatformPlanSummariesAsync(cancellationToken);
        var details = new List<DynamicSaasPlanDto>();
        foreach (var summary in summaries)
        {
            var detail = await _planRepository.GetPlanDetailAsync(summary.Id, cancellationToken);
            if (detail is not null)
            {
                detail.ActiveSubscriberCount = summary.ActiveSubscriberCount;
                detail.FeatureCount = summary.FeatureCount;
                detail.PricingOptionCount = summary.PricingOptionCount;
                details.Add(detail);
            }
        }

        return details;
    }

    public async Task<DynamicSaasPlanDto> GetPlanDetailAsync(int saasPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetPlanDetailAsync(saasPlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan not found.");

        var summaries = await _planRepository.GetPlatformPlanSummariesAsync(cancellationToken);
        var summary = summaries.FirstOrDefault(s => s.Id == saasPlanId);
        if (summary is not null)
        {
            plan.ActiveSubscriberCount = summary.ActiveSubscriberCount;
            plan.FeatureCount = summary.FeatureCount;
            plan.PricingOptionCount = summary.PricingOptionCount;
        }

        return plan;
    }

    public async Task<DynamicSaasPlanDto> CreatePlanAsync(CreateDynamicPlanDto dto, CancellationToken cancellationToken = default)
    {
        await _featureDependencyService.ValidateFeatureSelectionOrThrowAsync(dto.FeatureIds, cancellationToken);
        var planId = await _planRepository.CreatePlanAsync(dto, cancellationToken);
        return await GetPlanDetailAsync(planId, cancellationToken);
    }

    public async Task<DynamicSaasPlanDto> UpdatePlanAsync(int saasPlanId, UpdateDynamicPlanDto dto, CancellationToken cancellationToken = default)
    {
        await _featureDependencyService.ValidateFeatureSelectionOrThrowAsync(dto.FeatureIds, cancellationToken);
        await _planRepository.UpdatePlanAsync(saasPlanId, dto, cancellationToken);
        return await GetPlanDetailAsync(saasPlanId, cancellationToken);
    }

    public async Task<DynamicSaasPlanDto> ClonePlanAsync(int sourceSaasPlanId, CloneDynamicPlanDto dto, CancellationToken cancellationToken = default)
    {
        _ = await GetPlanDetailAsync(sourceSaasPlanId, cancellationToken);
        var planId = await _planRepository.ClonePlanAsync(sourceSaasPlanId, dto, cancellationToken);
        return await GetPlanDetailAsync(planId, cancellationToken);
    }

    public Task DeletePlanAsync(int saasPlanId, CancellationToken cancellationToken = default) =>
        _planRepository.DeletePlanAsync(saasPlanId, cancellationToken);

    public async Task<PlanPricingOptionDto> CreatePricingOptionAsync(
        int saasPlanId,
        CreatePlanPricingOptionDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureUniqueDurationAsync(saasPlanId, dto.DurationValue, dto.DurationUnit, null, cancellationToken);
        var id = await _planRepository.CreatePricingOptionAsync(saasPlanId, dto, cancellationToken);
        return (await _planRepository.GetPricingOptionByIdAsync(id, cancellationToken))!;
    }

    public async Task<PlanPricingOptionDto> UpdatePricingOptionAsync(
        int pricingOptionId,
        UpdatePlanPricingOptionDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _planRepository.GetPricingOptionByIdAsync(pricingOptionId, cancellationToken)
            ?? throw new KeyNotFoundException("Pricing option not found.");

        await EnsureUniqueDurationAsync(existing.SaasPlanId, dto.DurationValue, dto.DurationUnit, pricingOptionId, cancellationToken);
        await _planRepository.UpdatePricingOptionAsync(pricingOptionId, dto, cancellationToken);
        return (await _planRepository.GetPricingOptionByIdAsync(pricingOptionId, cancellationToken))!;
    }

    public Task DeletePricingOptionAsync(int pricingOptionId, CancellationToken cancellationToken = default) =>
        _planRepository.DeletePricingOptionAsync(pricingOptionId, cancellationToken);

    public async Task ReorderPricingOptionsAsync(
        int saasPlanId,
        ReorderPlanPricingOptionsDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Items.Count == 0)
            return;

        await _planRepository.ReorderPricingOptionsAsync(saasPlanId, dto.Items, cancellationToken);
    }

    public Task<FeatureDependencyValidationResult> ValidateFeaturesAsync(
        ValidatePlanFeaturesDto dto,
        CancellationToken cancellationToken = default) =>
        ValidateFeatureIdsAsync(dto.FeatureIds, cancellationToken);

    public Task<SaasPlanCatalogDto> GetPlanCatalogAsync(CancellationToken cancellationToken = default) =>
        _planRepository.GetPlanCatalogAsync(publicOnly: true, cancellationToken);

    public async Task<GymFeaturesDto> GetMyFeaturesAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        var subscriptionFeatures = await _featureResolver.GetSubscriptionFeatureCodesAsync(scope, cancellationToken);
        var enabledFeatures = await _featureResolver.GetEnabledFeatureCodesAsync(scope, cancellationToken);
        var visibleMenus = await _featureResolver.GetVisibleMenuCodesAsync(scope, cancellationToken);

        return new GymFeaturesDto
        {
            SubscriptionFeatureCodes = subscriptionFeatures,
            EnabledFeatureCodes = enabledFeatures,
            VisibleMenuCodes = visibleMenus
        };
    }

    private async Task EnsureUniqueDurationAsync(
        int saasPlanId,
        int durationValue,
        string durationUnit,
        int? excludePricingOptionId,
        CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetPlanDetailAsync(saasPlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan not found.");

        var normalizedUnit = durationUnit.Trim();
        var duplicate = plan.PricingOptions.Any(p =>
            p.IsActive
            && p.PricingOptionId != excludePricingOptionId
            && p.DurationValue == durationValue
            && string.Equals(p.DurationUnit, normalizedUnit, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            throw new InvalidOperationException($"A pricing option for {durationValue} {normalizedUnit} already exists on this plan.");
    }

    private async Task<FeatureDependencyValidationResult> ValidateFeatureIdsAsync(
        IReadOnlyList<int> featureIds,
        CancellationToken cancellationToken)
    {
        if (featureIds.Count == 0)
            return new FeatureDependencyValidationResult { IsValid = true };

        var allFeatures = await _featureRepository.GetAllFeaturesAsync(cancellationToken: cancellationToken);
        var codeById = allFeatures.ToDictionary(f => f.FeatureId, f => f.FeatureCode);
        var selectedCodes = featureIds.Where(codeById.ContainsKey).Select(id => codeById[id]);
        return await _featureDependencyService.ValidateFeatureSelectionAsync(selectedCodes, cancellationToken);
    }
}

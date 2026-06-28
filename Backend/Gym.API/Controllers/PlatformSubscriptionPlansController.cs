using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/platform/subscription-plans")]
[Authorize]
public class PlatformSubscriptionPlansController : ControllerBase
{
    private readonly IPlanManagementService _planManagementService;
    private readonly IFeatureDependencyService _featureDependencyService;

    public PlatformSubscriptionPlansController(
        IPlanManagementService planManagementService,
        IFeatureDependencyService featureDependencyService)
    {
        _planManagementService = planManagementService;
        _featureDependencyService = featureDependencyService;
    }

    [HttpGet("features")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SystemFeatureDto>>>> GetFeatures(CancellationToken cancellationToken)
    {
        var features = await _planManagementService.GetFeaturesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SystemFeatureDto>>.Ok(features));
    }

    [HttpGet("feature-dependencies")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyDictionary<string, IReadOnlyList<string>>>>> GetFeatureDependencies(
        CancellationToken cancellationToken)
    {
        var map = await _featureDependencyService.GetDependencyMapAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyDictionary<string, IReadOnlyList<string>>>.Ok(map));
    }

    [HttpPost("validate-features")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<FeatureDependencyValidationResult>>> ValidateFeatures(
        [FromBody] ValidatePlanFeaturesDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _planManagementService.ValidateFeaturesAsync(dto, cancellationToken);
        return Ok(ApiResponse<FeatureDependencyValidationResult>.Ok(result));
    }

    [HttpGet]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlanSummaryDto>>>> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _planManagementService.GetPlatformPlanSummariesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PlanSummaryDto>>.Ok(plans));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<DynamicSaasPlanDto>>> GetPlan(int id, CancellationToken cancellationToken)
    {
        var plan = await _planManagementService.GetPlanDetailAsync(id, cancellationToken);
        return Ok(ApiResponse<DynamicSaasPlanDto>.Ok(plan));
    }

    [HttpPost]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<DynamicSaasPlanDto>>> CreatePlan(
        [FromBody] CreateDynamicPlanDto dto,
        CancellationToken cancellationToken)
    {
        var plan = await _planManagementService.CreatePlanAsync(dto, cancellationToken);
        return Ok(ApiResponse<DynamicSaasPlanDto>.Ok(plan, "Plan created successfully."));
    }

    [HttpPost("{id:int}/clone")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<DynamicSaasPlanDto>>> ClonePlan(
        int id,
        [FromBody] CloneDynamicPlanDto dto,
        CancellationToken cancellationToken)
    {
        var plan = await _planManagementService.ClonePlanAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<DynamicSaasPlanDto>.Ok(plan, "Plan cloned successfully."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<DynamicSaasPlanDto>>> UpdatePlan(
        int id,
        [FromBody] UpdateDynamicPlanDto dto,
        CancellationToken cancellationToken)
    {
        var plan = await _planManagementService.UpdatePlanAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<DynamicSaasPlanDto>.Ok(plan, "Plan updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<object>>> DeletePlan(int id, CancellationToken cancellationToken)
    {
        await _planManagementService.DeletePlanAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Plan deactivated successfully."));
    }

    [HttpPost("{planId:int}/pricing-options")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<PlanPricingOptionDto>>> CreatePricingOption(
        int planId,
        [FromBody] CreatePlanPricingOptionDto dto,
        CancellationToken cancellationToken)
    {
        var option = await _planManagementService.CreatePricingOptionAsync(planId, dto, cancellationToken);
        return Ok(ApiResponse<PlanPricingOptionDto>.Ok(option, "Pricing option created."));
    }

    [HttpPut("pricing-options/{pricingOptionId:int}")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<PlanPricingOptionDto>>> UpdatePricingOption(
        int pricingOptionId,
        [FromBody] UpdatePlanPricingOptionDto dto,
        CancellationToken cancellationToken)
    {
        var option = await _planManagementService.UpdatePricingOptionAsync(pricingOptionId, dto, cancellationToken);
        return Ok(ApiResponse<PlanPricingOptionDto>.Ok(option, "Pricing option updated."));
    }

    [HttpPut("{planId:int}/pricing-options/reorder")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<object>>> ReorderPricingOptions(
        int planId,
        [FromBody] ReorderPlanPricingOptionsDto dto,
        CancellationToken cancellationToken)
    {
        await _planManagementService.ReorderPricingOptionsAsync(planId, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Pricing options reordered."));
    }

    [HttpDelete("pricing-options/{pricingOptionId:int}")]
    [RequirePermission(Permissions.ManageSubscriptionPlans)]
    public async Task<ActionResult<ApiResponse<object>>> DeletePricingOption(
        int pricingOptionId,
        CancellationToken cancellationToken)
    {
        await _planManagementService.DeletePricingOptionAsync(pricingOptionId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Pricing option deactivated."));
    }
}

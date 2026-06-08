using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/onboarding")]
public class GymOnboardingController : ControllerBase
{
    private readonly IGymOnboardingService _onboardingService;

    public GymOnboardingController(IGymOnboardingService onboardingService) => _onboardingService = onboardingService;

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterGymResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RegisterGymResultDto>>> Register(
        [FromBody] RegisterGymDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _onboardingService.RegisterGymAsync(dto, cancellationToken);
        return Ok(ApiResponse<RegisterGymResultDto>.Ok(result, result.Message));
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SaasPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SaasPlanDto>>>> GetPlans(
        [FromServices] ISaasSubscriptionService subscriptionService,
        CancellationToken cancellationToken)
    {
        var plans = await subscriptionService.GetPlansAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SaasPlanDto>>.Ok(plans));
    }
}

[ApiController]
[Route("api/saas")]
[Authorize]
public class SaasSubscriptionsController : ControllerBase
{
    private readonly ISaasSubscriptionService _subscriptionService;
    private readonly IGymBrandingService _brandingService;

    public SaasSubscriptionsController(ISaasSubscriptionService subscriptionService, IGymBrandingService brandingService)
    {
        _subscriptionService = subscriptionService;
        _brandingService = brandingService;
    }

    [HttpGet("subscription")]
    [RequirePermission(Permissions.ViewSaasSubscription)]
    public async Task<ActionResult<ApiResponse<GymSubscriptionDto>>> GetSubscription(
        [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var sub = await _subscriptionService.GetCurrentSubscriptionAsync(gymId, cancellationToken);
        return Ok(ApiResponse<GymSubscriptionDto>.Ok(sub));
    }

    [HttpGet("usage")]
    [RequirePermission(Permissions.ViewSaasSubscription)]
    public async Task<ActionResult<ApiResponse<GymUsageDto>>> GetUsage(
        [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var usage = await _subscriptionService.GetUsageAsync(gymId, cancellationToken);
        return Ok(ApiResponse<GymUsageDto>.Ok(usage));
    }

    [HttpGet("plans")]
    [RequirePermission(Permissions.ViewSaasSubscription)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SaasPlanDto>>>> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _subscriptionService.GetPlansAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SaasPlanDto>>.Ok(plans));
    }

    [HttpPost("payments/order")]
    [RequirePermission(Permissions.ManageSaasSubscription)]
    public async Task<ActionResult<ApiResponse<SaasPaymentOrderResponseDto>>> CreateOrder(
        [FromBody] CreateSaasPaymentOrderDto dto,
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var order = await _subscriptionService.CreatePaymentOrderAsync(dto, gymId, cancellationToken);
        return Ok(ApiResponse<SaasPaymentOrderResponseDto>.Ok(order));
    }

    [HttpPost("payments/verify")]
    [RequirePermission(Permissions.ManageSaasSubscription)]
    public async Task<ActionResult<ApiResponse<GymSubscriptionDto>>> VerifyPayment(
        [FromBody] VerifySaasPaymentDto dto,
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var sub = await _subscriptionService.VerifyPaymentAsync(dto, gymId, cancellationToken);
        return Ok(ApiResponse<GymSubscriptionDto>.Ok(sub, "Subscription activated successfully."));
    }

    [HttpPost("subscription/cancel")]
    [RequirePermission(Permissions.ManageSaasSubscription)]
    public async Task<ActionResult<ApiResponse<GymSubscriptionDto>>> Cancel(
        [FromQuery] bool cancelAtPeriodEnd = true,
        [FromQuery] Guid? gymId = null,
        CancellationToken cancellationToken = default)
    {
        var sub = await _subscriptionService.CancelAsync(cancelAtPeriodEnd, gymId, cancellationToken);
        return Ok(ApiResponse<GymSubscriptionDto>.Ok(sub));
    }

    [HttpGet("branding")]
    [RequirePermission(Permissions.ManageGymBranding)]
    public async Task<ActionResult<ApiResponse<GymBrandingDto>>> GetBranding(
        [FromQuery] Guid? gymId, CancellationToken cancellationToken)
    {
        var branding = await _brandingService.GetBrandingAsync(gymId, cancellationToken);
        return Ok(ApiResponse<GymBrandingDto>.Ok(branding));
    }

    [HttpPut("branding")]
    [RequirePermission(Permissions.ManageGymBranding)]
    public async Task<ActionResult<ApiResponse<GymBrandingDto>>> UpdateBranding(
        [FromBody] UpdateGymBrandingDto dto,
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var branding = await _brandingService.UpdateBrandingAsync(dto, gymId, cancellationToken);
        return Ok(ApiResponse<GymBrandingDto>.Ok(branding));
    }
}

[ApiController]
[Route("api/saas/platform")]
[Authorize]
public class SaasPlatformController : ControllerBase
{
    private readonly ISaasSubscriptionService _subscriptionService;

    public SaasPlatformController(ISaasSubscriptionService subscriptionService) =>
        _subscriptionService = subscriptionService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewPlatformSaas)]
    public async Task<ActionResult<ApiResponse<SaasPlatformDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _subscriptionService.GetPlatformDashboardAsync(cancellationToken);
        return Ok(ApiResponse<SaasPlatformDashboardDto>.Ok(dashboard));
    }
}

using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.WhiteLabel;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/white-label")]
[Authorize]
public class WhiteLabelController : ControllerBase
{
    private readonly IWhiteLabelService _whiteLabelService;

    public WhiteLabelController(IWhiteLabelService whiteLabelService) => _whiteLabelService = whiteLabelService;

    [HttpGet("settings")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ViewWhiteLabel)]
    public async Task<ActionResult<ApiResponse<WhiteLabelSettingsDto>>> GetSettings(CancellationToken cancellationToken) =>
        Ok(ApiResponse<WhiteLabelSettingsDto>.Ok(await _whiteLabelService.GetSettingsAsync(cancellationToken)));

    [HttpPut("settings")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<WhiteLabelSettingsDto>>> UpsertSettings(
        [FromBody] UpsertWhiteLabelSettingsDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<WhiteLabelSettingsDto>.Ok(await _whiteLabelService.UpsertSettingsAsync(dto, cancellationToken)));

    [HttpPost("settings/enable")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<object>>> Enable(CancellationToken cancellationToken)
    {
        await _whiteLabelService.EnableAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "White label enabled."));
    }

    [HttpPost("settings/disable")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<object>>> Disable(CancellationToken cancellationToken)
    {
        await _whiteLabelService.DisableAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "White label disabled."));
    }

    [HttpPut("domain")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateDomain(
        [FromBody] UpdateWhiteLabelDomainDto dto, CancellationToken cancellationToken)
    {
        await _whiteLabelService.UpdateDomainAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Domain updated."));
    }

    [HttpGet("email-templates")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ViewWhiteLabel)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WhiteLabelEmailTemplateDto>>>> GetEmailTemplates(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<WhiteLabelEmailTemplateDto>>.Ok(await _whiteLabelService.GetEmailTemplatesAsync(cancellationToken)));

    [HttpPost("email-templates")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<WhiteLabelEmailTemplateDto>>> CreateEmailTemplate(
        [FromBody] UpsertWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<WhiteLabelEmailTemplateDto>.Ok(await _whiteLabelService.CreateEmailTemplateAsync(dto, cancellationToken)));

    [HttpPut("email-templates/{id:int}")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<WhiteLabelEmailTemplateDto>>> UpdateEmailTemplate(
        int id, [FromBody] UpsertWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken)
    {
        var update = new UpdateWhiteLabelEmailTemplateDto
        {
            Id = id,
            TemplateName = dto.TemplateName,
            Subject = dto.Subject,
            Body = dto.Body,
            IsActive = dto.IsActive
        };
        return Ok(ApiResponse<WhiteLabelEmailTemplateDto>.Ok(await _whiteLabelService.UpdateEmailTemplateAsync(update, cancellationToken)));
    }

    [HttpDelete("email-templates/{id:int}")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEmailTemplate(int id, CancellationToken cancellationToken)
    {
        await _whiteLabelService.DeleteEmailTemplateAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Template deleted."));
    }

    [HttpGet("mobile-settings")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ViewWhiteLabel)]
    public async Task<ActionResult<ApiResponse<WhiteLabelMobileSettingsDto>>> GetMobileSettings(CancellationToken cancellationToken)
    {
        var settings = await _whiteLabelService.GetMobileSettingsAsync(cancellationToken)
            ?? new WhiteLabelMobileSettingsDto();
        return Ok(ApiResponse<WhiteLabelMobileSettingsDto>.Ok(settings));
    }

    [HttpPut("mobile-settings")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ManageWhiteLabel)]
    public async Task<ActionResult<ApiResponse<WhiteLabelMobileSettingsDto>>> UpsertMobileSettings(
        [FromBody] UpsertWhiteLabelMobileSettingsDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<WhiteLabelMobileSettingsDto>.Ok(await _whiteLabelService.UpsertMobileSettingsAsync(dto, cancellationToken)));

    [HttpGet("preview")]
    [RequireFeature("WHITE_LABEL")]
    [RequirePermission(Permissions.ViewWhiteLabel)]
    public async Task<ActionResult<ApiResponse<WhiteLabelPreviewDto>>> GetPreview(CancellationToken cancellationToken) =>
        Ok(ApiResponse<WhiteLabelPreviewDto>.Ok(await _whiteLabelService.GetPreviewAsync(cancellationToken)));

    /// <summary>Branding for authenticated gym portals (admin, trainer, member). Same source as preview login.</summary>
    [HttpGet("app-branding")]
    public async Task<ActionResult<ApiResponse<WhiteLabelLoginBrandingDto>>> GetAppBranding(CancellationToken cancellationToken) =>
        Ok(ApiResponse<WhiteLabelLoginBrandingDto>.Ok(await _whiteLabelService.GetAppBrandingAsync(cancellationToken)));
}

[ApiController]
[Route("api/public/white-label")]
[AllowAnonymous]
public class PublicWhiteLabelController : ControllerBase
{
    private readonly IWhiteLabelService _whiteLabelService;

    public PublicWhiteLabelController(IWhiteLabelService whiteLabelService) => _whiteLabelService = whiteLabelService;

    [HttpGet("login-branding")]
    public async Task<ActionResult<ApiResponse<WhiteLabelLoginBrandingDto>>> GetLoginBranding(
        [FromQuery] Guid? gymId,
        [FromQuery] string? subDomain,
        [FromQuery] string? customDomain,
        CancellationToken cancellationToken)
    {
        var branding = await _whiteLabelService.GetLoginBrandingAsync(new WhiteLabelLoginBrandingQueryDto
        {
            GymId = gymId,
            SubDomain = subDomain,
            CustomDomain = customDomain
        }, cancellationToken);

        if (branding is null)
            return NotFound(ApiResponse<WhiteLabelLoginBrandingDto>.Fail("Branding not found."));

        return Ok(ApiResponse<WhiteLabelLoginBrandingDto>.Ok(branding));
    }
}

[ApiController]
[Route("api/platform/white-label")]
[Authorize]
public class WhiteLabelPlatformController : ControllerBase
{
    private readonly IWhiteLabelService _whiteLabelService;

    public WhiteLabelPlatformController(IWhiteLabelService whiteLabelService) => _whiteLabelService = whiteLabelService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewPlatformSaas)]
    public async Task<ActionResult<ApiResponse<WhiteLabelPlatformDashboardDto>>> GetDashboard(CancellationToken cancellationToken) =>
        Ok(ApiResponse<WhiteLabelPlatformDashboardDto>.Ok(await _whiteLabelService.GetPlatformDashboardAsync(cancellationToken)));
}

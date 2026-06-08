using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Mobile;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/mobile")]
[Authorize]
public class MobileController : ControllerBase
{
    private readonly IMobilePushService _mobilePushService;

    public MobileController(IMobilePushService mobilePushService) => _mobilePushService = mobilePushService;

    [HttpPost("device/register")]
    public async Task<ActionResult<ApiResponse<object>>> RegisterDevice([FromBody] RegisterDeviceDto dto, CancellationToken cancellationToken)
    {
        await _mobilePushService.RegisterDeviceAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Device registered."));
    }

    [HttpPost("device/unregister")]
    public async Task<ActionResult<ApiResponse<object>>> UnregisterDevice([FromBody] UnregisterDeviceDto dto, CancellationToken cancellationToken)
    {
        await _mobilePushService.UnregisterDeviceAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Device unregistered."));
    }

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewMemberDashboard)]
    public async Task<ActionResult<ApiResponse<MobileDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mobilePushService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<MobileDashboardDto>.Ok(result));
    }

    [HttpGet("sync")]
    public async Task<ActionResult<ApiResponse<object>>> GetSync([FromQuery] DateTime? lastSyncDate, CancellationToken cancellationToken)
    {
        if (lastSyncDate.HasValue)
        {
            var delta = await _mobilePushService.GetSyncDeltaAsync(new MobileSyncQueryDto { LastSyncDate = lastSyncDate }, cancellationToken);
            return Ok(ApiResponse<MobileSyncDeltaDto>.Ok(delta));
        }

        var sync = await _mobilePushService.GetSyncAsync(cancellationToken);
        return Ok(ApiResponse<MobileSyncDto>.Ok(sync));
    }

    [HttpGet("notifications")]
    [RequirePermission(Permissions.ViewMobileNotifications)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<PushNotificationDto>>>> GetNotifications(
        [FromQuery] PushNotificationQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await _mobilePushService.GetNotificationsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<PushNotificationDto>>.Ok(result));
    }

    [HttpPut("notifications/read")]
    [RequirePermission(Permissions.ViewMobileNotifications)]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead([FromBody] MarkNotificationsReadDto dto, CancellationToken cancellationToken)
    {
        await _mobilePushService.MarkNotificationsReadAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Notifications marked as read."));
    }

    [HttpPost("notifications/{id:int}/engagement")]
    [RequirePermission(Permissions.ViewMobileNotifications)]
    public async Task<ActionResult<ApiResponse<object>>> RecordEngagement(int id, [FromBody] RecordPushEngagementDto dto, CancellationToken cancellationToken)
    {
        await _mobilePushService.RecordEngagementAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Engagement recorded."));
    }

    [HttpGet("preferences")]
    [RequirePermission(Permissions.ManageNotificationPreferences)]
    public async Task<ActionResult<ApiResponse<NotificationPreferencesDto>>> GetPreferences(CancellationToken cancellationToken)
    {
        var result = await _mobilePushService.GetPreferencesAsync(cancellationToken);
        return Ok(ApiResponse<NotificationPreferencesDto>.Ok(result));
    }

    [HttpPut("preferences")]
    [RequirePermission(Permissions.ManageNotificationPreferences)]
    public async Task<ActionResult<ApiResponse<object>>> UpdatePreferences([FromBody] UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken)
    {
        await _mobilePushService.UpdatePreferencesAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Preferences updated."));
    }
}

[ApiController]
[Route("api/mobile/admin")]
[Authorize]
public class MobileAdminController : ControllerBase
{
    private readonly IMobilePushService _mobilePushService;

    public MobileAdminController(IMobilePushService mobilePushService) => _mobilePushService = mobilePushService;

    [HttpPost("send")]
    [RequirePermission(Permissions.SendNotifications)]
    public async Task<ActionResult<ApiResponse<object>>> SendCampaign([FromBody] SendPushCampaignDto dto, CancellationToken cancellationToken)
    {
        await _mobilePushService.SendCampaignAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Push campaign queued."));
    }

    [HttpGet("analytics")]
    [RequirePermission(Permissions.ViewNotifications)]
    public async Task<ActionResult<ApiResponse<PushNotificationAnalyticsDto>>> GetAnalytics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _mobilePushService.GetAnalyticsAsync(fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<PushNotificationAnalyticsDto>.Ok(result));
    }

    [HttpGet("campaigns")]
    [RequirePermission(Permissions.ViewNotifications)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<PushCampaignHistoryDto>>>> GetCampaigns(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mobilePushService.GetCampaignHistoryAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<PushCampaignHistoryDto>>.Ok(result));
    }
}

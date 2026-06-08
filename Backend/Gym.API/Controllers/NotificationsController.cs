using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService) =>
        _notificationService = notificationService;

    [HttpGet("dashboard")]
    [RequirePermission(Permissions.ViewNotifications)]
    public async Task<ActionResult<ApiResponse<NotificationDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _notificationService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<NotificationDashboardDto>.Ok(dashboard));
    }

    [HttpGet("templates")]
    [RequirePermission(Permissions.ViewNotifications)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationTemplateDto>>>> GetTemplates(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var templates = await _notificationService.GetTemplatesAsync(includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationTemplateDto>>.Ok(templates));
    }

    [HttpPost("templates")]
    [RequirePermission(Permissions.ManageNotifications)]
    public async Task<ActionResult<ApiResponse<NotificationTemplateDto>>> CreateTemplate(
        [FromBody] CreateNotificationTemplateDto dto,
        CancellationToken cancellationToken)
    {
        var template = await _notificationService.CreateTemplateAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<NotificationTemplateDto>.Ok(template, "Template created."));
    }

    [HttpPut("templates/{id:int}")]
    [RequirePermission(Permissions.ManageNotifications)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTemplate(
        int id,
        [FromBody] UpdateNotificationTemplateDto dto,
        CancellationToken cancellationToken)
    {
        await _notificationService.UpdateTemplateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Template updated."));
    }

    [HttpDelete("templates/{id:int}")]
    [RequirePermission(Permissions.ManageNotifications)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTemplate(int id, CancellationToken cancellationToken)
    {
        await _notificationService.DeleteTemplateAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Template deleted."));
    }

    [HttpGet("settings")]
    [RequirePermission(Permissions.ViewNotifications)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationSettingDto>>>> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await _notificationService.GetSettingsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationSettingDto>>.Ok(settings));
    }

    [HttpPut("settings")]
    [RequirePermission(Permissions.ManageNotifications)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSettings(
        [FromBody] IReadOnlyList<UpdateNotificationSettingDto> settings,
        CancellationToken cancellationToken)
    {
        await _notificationService.UpdateSettingsAsync(settings, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Settings updated."));
    }

    [HttpGet("history")]
    [RequirePermission(Permissions.ViewNotifications)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<NotificationLogDto>>>> GetHistory(
        [FromQuery] NotificationSearchQueryDto query,
        CancellationToken cancellationToken)
    {
        var history = await _notificationService.SearchHistoryAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResultDto<NotificationLogDto>>.Ok(history));
    }

    [HttpPost("test")]
    [RequirePermission(Permissions.SendNotifications)]
    public async Task<ActionResult<ApiResponse<NotificationLogDto>>> SendTest(
        [FromBody] SendTestNotificationDto dto,
        CancellationToken cancellationToken)
    {
        var log = await _notificationService.SendTestAsync(dto, cancellationToken);
        return Ok(ApiResponse<NotificationLogDto>.Ok(log, "Test notification sent."));
    }

    [HttpPost("send")]
    [RequirePermission(Permissions.SendNotifications)]
    public async Task<ActionResult<ApiResponse<object>>> Send(
        [FromBody] SendNotificationRequestDto dto,
        CancellationToken cancellationToken)
    {
        await _notificationService.QueueAndSendAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Notification queued."));
    }
}

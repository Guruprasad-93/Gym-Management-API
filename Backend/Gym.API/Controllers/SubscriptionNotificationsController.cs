using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/subscription-notifications")]
[Authorize]
public class SubscriptionNotificationsController : ControllerBase
{
    private readonly ISubscriptionNotificationService _notificationService;

    public SubscriptionNotificationsController(ISubscriptionNotificationService notificationService) =>
        _notificationService = notificationService;

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<UserInAppNotificationsResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserInAppNotificationsResponseDto>>> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetMyNotificationsAsync(unreadOnly, cancellationToken);
        return Ok(ApiResponse<UserInAppNotificationsResponseDto>.Ok(result));
    }

    [HttpPut("read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(
        [FromBody] MarkUserInAppNotificationsReadDto dto,
        CancellationToken cancellationToken = default)
    {
        await _notificationService.MarkReadAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Notifications marked as read."));
    }
}

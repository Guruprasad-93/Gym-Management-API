using Gym.Application.DTOs.Notifications;
using Gym.Application.DTOs.Saas;

namespace Gym.Application.Interfaces;

public interface ISubscriptionNotificationRepository
{
    Task<bool> CreateNotificationAsync(
        Guid gymId,
        Guid userId,
        string notificationKey,
        string notificationType,
        string title,
        string message,
        string severity,
        string? actionRoute,
        bool showLoginPopup,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserInAppNotificationDto>> GetForUserAsync(
        Guid gymId,
        Guid userId,
        bool unreadOnly,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid gymId, Guid userId, CancellationToken cancellationToken = default);

    Task MarkReadAsync(Guid gymId, Guid userId, IReadOnlyList<int>? notificationIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GymSubscriptionDto>> GetActiveSubscriptionsForNotificationJobAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionTenantUserDto>> GetGymTenantUsersAsync(
        Guid gymId,
        CancellationToken cancellationToken = default);
}

public class SubscriptionTenantUserDto
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public interface ISubscriptionNotificationService
{
    Task<UserInAppNotificationsResponseDto> GetMyNotificationsAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default);

    Task MarkReadAsync(MarkUserInAppNotificationsReadDto dto, CancellationToken cancellationToken = default);

    Task<SubscriptionExpiryNotificationGenerationResultDto> GenerateDailyNotificationsAsync(
        CancellationToken cancellationToken = default);
}

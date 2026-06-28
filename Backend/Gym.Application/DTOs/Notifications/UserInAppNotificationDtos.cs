namespace Gym.Application.DTOs.Notifications;

public class UserInAppNotificationDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public Guid UserId { get; set; }
    public string NotificationKey { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? ActionRoute { get; set; }
    public bool ShowLoginPopup { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class UserInAppNotificationsResponseDto
{
    public IReadOnlyList<UserInAppNotificationDto> Items { get; set; } = Array.Empty<UserInAppNotificationDto>();
    public int UnreadCount { get; set; }
}

public class MarkUserInAppNotificationsReadDto
{
    public IReadOnlyList<int>? NotificationIds { get; set; }
}

public class SubscriptionExpiryNotificationGenerationResultDto
{
    public int SubscriptionsProcessed { get; set; }
    public int NotificationsCreated { get; set; }
    public int NotificationsSkippedDuplicate { get; set; }
}

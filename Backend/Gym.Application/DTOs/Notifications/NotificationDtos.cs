namespace Gym.Application.DTOs.Notifications;

public class NotificationTemplateDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? BodyTemplate { get; set; }
    public string? VariablesJson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateNotificationTemplateDto
{
    public string NotificationType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? BodyTemplate { get; set; }
    public string? VariablesJson { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateNotificationTemplateDto : CreateNotificationTemplateDto;

public class NotificationSettingDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? ProviderTemplateName { get; set; }
}

public class UpdateNotificationSettingDto
{
    public string NotificationType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? ProviderTemplateName { get; set; }
}

public class NotificationLogDto
{
    public long Id { get; set; }
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string RecipientPhone { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public int? MemberId { get; set; }
    public string WhatsAppTemplateName { get; set; } = string.Empty;
    public string? VariablesJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? ProviderMessageId { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
}

public class NotificationSearchQueryDto
{
    public string? Search { get; set; }
    public string? NotificationType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class NotificationDashboardDto
{
    public int TotalLogs { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public int ActiveTemplates { get; set; }
    public int SentToday { get; set; }
}

public class SendTestNotificationDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string? TemplateName { get; set; }
    public Dictionary<string, string>? Variables { get; set; }
}

public class SendNotificationRequestDto
{
    public string NotificationType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? RecipientUserId { get; set; }
    public int? MemberId { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
}

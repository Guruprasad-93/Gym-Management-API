namespace Gym.Infrastructure.Persistence.Models;

internal sealed class NotificationTemplateRow
{
    public int TemplateId { get; set; }
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? BodyTemplate { get; set; }
    public string? VariablesJson { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

internal sealed class NotificationSettingRow
{
    public int SettingId { get; set; }
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? ProviderTemplateName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

internal sealed class NotificationLogRow
{
    public long LogId { get; set; }
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string RecipientPhone { get; set; } = string.Empty;
    public Guid? RecipientUserId { get; set; }
    public int? MemberId { get; set; }
    public string WhatsAppTemplateName { get; set; } = string.Empty;
    public string? VariablesJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? ProviderMessageId { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RecipientName { get; set; }
}

internal sealed class NotificationDashboardRow
{
    public int TotalLogs { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public int ActiveTemplates { get; set; }
    public int SentToday { get; set; }
}

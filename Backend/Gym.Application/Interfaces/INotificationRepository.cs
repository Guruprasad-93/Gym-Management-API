using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Notifications;

namespace Gym.Application.Interfaces;

public interface INotificationRepository
{
    Task<NotificationTemplateDto> CreateTemplateAsync(Guid gymId, CreateNotificationTemplateDto dto, CancellationToken cancellationToken = default);
    Task UpdateTemplateAsync(int templateId, Guid gymId, UpdateNotificationTemplateDto dto, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(int templateId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationTemplateDto>> GetTemplatesAsync(Guid gymId, bool includeInactive, CancellationToken cancellationToken = default);
    Task UpsertSettingAsync(Guid gymId, UpdateNotificationSettingDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationSettingDto>> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<long> LogNotificationAsync(LogNotificationCommand command, CancellationToken cancellationToken = default);
    Task UpdateLogStatusAsync(long logId, Guid gymId, string status, string? errorMessage, string? providerMessageId, DateTime? sentAt, CancellationToken cancellationToken = default);
    Task<PagedResultDto<NotificationLogDto>> SearchLogsAsync(Guid gymId, NotificationSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<NotificationDashboardDto> GetDashboardAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PendingNotificationRow>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpiryNotificationCandidateRow>> GetExpiryCandidatesAsync(Guid gymId, int daysBeforeExpiry, string notificationType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetActiveGymIdsAsync(CancellationToken cancellationToken = default);
}

public class LogNotificationCommand
{
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string RecipientPhone { get; set; } = string.Empty;
    public Guid? RecipientUserId { get; set; }
    public int? MemberId { get; set; }
    public string WhatsAppTemplateName { get; set; } = string.Empty;
    public string? VariablesJson { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public string? ProviderMessageId { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
}

public class PendingNotificationRow
{
    public long LogId { get; set; }
    public Guid GymId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string RecipientPhone { get; set; } = string.Empty;
    public string WhatsAppTemplateName { get; set; } = string.Empty;
    public string? VariablesJson { get; set; }
}

public class ExpiryNotificationCandidateRow
{
    public int MembershipId { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public DateTime EndDate { get; set; }
    public Guid RecipientUserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string? MemberEmail { get; set; }
    public string MemberPhone { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
}

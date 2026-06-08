using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Notifications;

namespace Gym.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateDto dto, CancellationToken cancellationToken = default);
    Task UpdateTemplateAsync(int templateId, UpdateNotificationTemplateDto dto, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(int templateId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationTemplateDto>> GetTemplatesAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task UpdateSettingsAsync(IReadOnlyList<UpdateNotificationSettingDto> settings, CancellationToken cancellationToken = default);
    Task<NotificationDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<NotificationLogDto>> SearchHistoryAsync(NotificationSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<NotificationLogDto> SendTestAsync(SendTestNotificationDto dto, CancellationToken cancellationToken = default);
    Task QueueAndSendAsync(SendNotificationRequestDto request, CancellationToken cancellationToken = default);
    Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default);
    Task QueueMembershipExpiryRemindersAsync(CancellationToken cancellationToken = default);
    Task SendEventNotificationAsync(Guid gymId, SendNotificationRequestDto request, CancellationToken cancellationToken = default);
}

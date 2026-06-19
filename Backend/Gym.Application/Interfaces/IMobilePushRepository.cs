using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Mobile;

namespace Gym.Application.Interfaces;

public interface IMobilePushRepository
{
    Task<int> RegisterDeviceAsync(Guid gymId, Guid userId, RegisterDeviceDto dto, CancellationToken cancellationToken = default);
    Task UnregisterDeviceAsync(Guid gymId, Guid userId, string deviceToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceTokenDto>> GetActiveDevicesAsync(Guid gymId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CreatePushNotificationAsync(Guid gymId, Guid userId, string title, string message, string notificationType, string? dataJson, CancellationToken cancellationToken = default);
    Task UpdatePushStatusAsync(int id, Guid gymId, string status, string? failureReason, DateTime? sentDate, DateTime? deliveredDate, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid gymId, Guid userId, IReadOnlyList<int>? notificationIds, CancellationToken cancellationToken = default);
    Task RecordEngagementAsync(int id, Guid gymId, Guid userId, string engagementType, CancellationToken cancellationToken = default);
    Task<PagedResultDto<PushNotificationDto>> GetNotificationsPagedAsync(Guid gymId, Guid userId, PushNotificationQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PendingPushRow>> GetPendingPushNotificationsAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<NotificationPreferencesDto> GetOrCreatePreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default);
    Task<bool> IsCategoryEnabledAsync(Guid userId, string notificationType, CancellationToken cancellationToken = default);
    Task<MobileDashboardDto> GetMobileDashboardAsync(Guid gymId, Guid userId, int memberId, CancellationToken cancellationToken = default);
    Task<MobileSyncProfileDto?> GetSyncProfileAsync(Guid gymId, Guid userId, CancellationToken cancellationToken = default);
    Task<MobileSyncDeltaDto> GetSyncDeltaAsync(Guid gymId, Guid userId, int memberId, DateTime lastSyncDate, CancellationToken cancellationToken = default);
    Task<PushNotificationAnalyticsDto> GetAnalyticsAsync(Guid gymId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<PagedResultDto<PushCampaignHistoryDto>> GetCampaignHistoryAsync(Guid gymId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipPushCandidateRow>> GetMembershipsExpiringForPushAsync(int daysUntilExpiry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberPushCandidateRow>> GetAttendanceReminderCandidatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberPushCandidateRow>> GetWorkoutReminderCandidatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberPushCandidateRow>> GetDietReminderCandidatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoalPushCandidateRow>> GetGoalReminderCandidatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetMemberUserIdsAsync(Guid gymId, int? branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetCampaignRecipientUserIdsAsync(
        Guid gymId,
        string targetAudience,
        int? branchId,
        int expiringWithinDays,
        IReadOnlyList<Guid>? userIds,
        CancellationToken cancellationToken = default);
}

public sealed class PendingPushRow
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string? DataJson { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
}

public sealed class MembershipPushCandidateRow
{
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public Guid UserId { get; set; }
    public int MembershipId { get; set; }
    public DateTime EndDate { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
}

public sealed class MemberPushCandidateRow
{
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public Guid UserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
}

public sealed class GoalPushCandidateRow
{
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public Guid UserId { get; set; }
    public int GoalId { get; set; }
    public string GoalType { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public string MemberName { get; set; } = string.Empty;
}

public interface IMobilePushService
{
    Task RegisterDeviceAsync(RegisterDeviceDto dto, CancellationToken cancellationToken = default);
    Task UnregisterDeviceAsync(UnregisterDeviceDto dto, CancellationToken cancellationToken = default);
    Task<MobileDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<MobileSyncDto> GetSyncAsync(CancellationToken cancellationToken = default);
    Task<MobileSyncDeltaDto> GetSyncDeltaAsync(MobileSyncQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<PushNotificationDto>> GetNotificationsAsync(PushNotificationQueryDto query, CancellationToken cancellationToken = default);
    Task MarkNotificationsReadAsync(MarkNotificationsReadDto dto, CancellationToken cancellationToken = default);
    Task RecordEngagementAsync(int notificationId, RecordPushEngagementDto dto, CancellationToken cancellationToken = default);
    Task<NotificationPreferencesDto> GetPreferencesAsync(CancellationToken cancellationToken = default);
    Task UpdatePreferencesAsync(UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default);
    Task SendEventPushAsync(Guid gymId, SendEventPushRequest request, CancellationToken cancellationToken = default);
    Task ProcessPendingPushNotificationsAsync(CancellationToken cancellationToken = default);
    Task QueueScheduledRemindersAsync(CancellationToken cancellationToken = default);
    Task SendCampaignAsync(SendPushCampaignDto dto, CancellationToken cancellationToken = default);
    Task<PushNotificationAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<PagedResultDto<PushCampaignHistoryDto>> GetCampaignHistoryAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

public interface IFirebasePushService
{
    Task<FirebasePushResult> SendAsync(FirebasePushMessage message, CancellationToken cancellationToken = default);
}

public sealed class FirebasePushMessage
{
    public string DeviceToken { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string>? Data { get; set; }
}

public sealed class FirebasePushResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

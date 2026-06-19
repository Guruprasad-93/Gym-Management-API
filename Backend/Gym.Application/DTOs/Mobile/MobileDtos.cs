namespace Gym.Application.DTOs.Mobile;

public class RegisterDeviceDto
{
    public string DeviceType { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
    public string? AppVersion { get; set; }
}

public class UnregisterDeviceDto
{
    public string DeviceToken { get; set; } = string.Empty;
}

public class DeviceTokenDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public Guid UserId { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
    public string? AppVersion { get; set; }
    public DateTime LastActiveDate { get; set; }
    public bool IsActive { get; set; }
}

public class PushNotificationDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string? DataJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? OpenedDate { get; set; }
    public DateTime? ClickedDate { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class MarkNotificationsReadDto
{
    public IReadOnlyList<int>? NotificationIds { get; set; }
}

public class NotificationPreferencesDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public bool PushEnabled { get; set; }
    public bool MembershipReminders { get; set; }
    public bool WorkoutReminders { get; set; }
    public bool DietReminders { get; set; }
    public bool AttendanceReminders { get; set; }
    public bool PromotionalNotifications { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class UpdateNotificationPreferencesDto
{
    public bool PushEnabled { get; set; } = true;
    public bool MembershipReminders { get; set; } = true;
    public bool WorkoutReminders { get; set; } = true;
    public bool DietReminders { get; set; } = true;
    public bool AttendanceReminders { get; set; } = true;
    public bool PromotionalNotifications { get; set; } = true;
}

public class MobileDashboardDto
{
    public MobileMembershipSummaryDto? Membership { get; set; }
    public MobileAttendanceSummaryDto Attendance { get; set; } = new();
    public MobileGoalSummaryDto? Goal { get; set; }
    public MobileWorkoutTodayDto? TodayWorkout { get; set; }
    public MobileDietTodayDto? TodayDiet { get; set; }
    public MobileWaterTodayDto? WaterToday { get; set; }
    public int UnreadNotificationCount { get; set; }
    public IReadOnlyList<PushNotificationDto> RecentNotifications { get; set; } = Array.Empty<PushNotificationDto>();
}

public class MobileMembershipSummaryDto
{
    public int MembershipId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RemainingDays { get; set; }
}

public class MobileAttendanceSummaryDto
{
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
}

public class MobileGoalSummaryDto
{
    public int GoalId { get; set; }
    public string GoalType { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime TargetDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ProgressPercent { get; set; }
}

public class MobileWorkoutTodayDto
{
    public int? WorkoutTrackingId { get; set; }
    public string? PlanName { get; set; }
    public DateTime? WorkoutDate { get; set; }
    public bool Completed { get; set; }
}

public class MobileDietTodayDto
{
    public int? DietTrackingId { get; set; }
    public string? PlanName { get; set; }
    public DateTime? TrackingDate { get; set; }
    public bool Completed { get; set; }
}

public class MobileWaterTodayDto
{
    public int? WaterIntakeId { get; set; }
    public decimal? AmountMl { get; set; }
    public DateTime? LogDate { get; set; }
}

public class MobileSyncDto
{
    public MobileSyncProfileDto Profile { get; set; } = new();
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public MobileFeatureFlagsDto FeatureFlags { get; set; } = new();
    public MobileBrandingDto Branding { get; set; } = new();
    public MobileAppSettingsDto AppSettings { get; set; } = new();
    public DateTime ServerTimeUtc { get; set; }
}

public class MobileSyncProfileDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public string? GymName { get; set; }
    public int? MemberId { get; set; }
    public string? Phone { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}

public class MobileFeatureFlagsDto
{
    public bool MemberSelfService { get; set; } = true;
    public bool OnlinePayments { get; set; } = true;
    public bool QrCheckIn { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool MultiBranch { get; set; } = true;
}

public class MobileBrandingDto
{
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}

public class MobileAppSettingsDto
{
    public string ApiVersion { get; set; } = "1.0";
    public int SyncIntervalMinutes { get; set; } = 15;
    public bool OfflineCacheEnabled { get; set; } = true;
}

public class MobileSyncDeltaDto
{
    public DateTime LastSyncDate { get; set; }
    public DateTime ServerTimeUtc { get; set; }
    public IReadOnlyList<PushNotificationDto> Notifications { get; set; } = Array.Empty<PushNotificationDto>();
    public IReadOnlyList<object> UpdatedGoals { get; set; } = Array.Empty<object>();
    public IReadOnlyList<object> UpdatedWaterLogs { get; set; } = Array.Empty<object>();
}

public class MobileSyncQueryDto
{
    public DateTime? LastSyncDate { get; set; }
}

public class PushNotificationQueryDto : Common.PagedRequestDto
{
    public bool UnreadOnly { get; set; }
}

public class SendPushCampaignDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TargetAudience { get; set; }
    public int? BranchId { get; set; }
    public int ExpiringWithinDays { get; set; } = 30;
    public IReadOnlyList<Guid>? UserIds { get; set; }
}

public class PushNotificationAnalyticsDto
{
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalFailed { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalPending { get; set; }
    public int ActiveDevices { get; set; }
    public IReadOnlyList<PushNotificationTypeStatDto> ByType { get; set; } = Array.Empty<PushNotificationTypeStatDto>();
}

public class PushNotificationTypeStatDto
{
    public string NotificationType { get; set; } = string.Empty;
    public int Count { get; set; }
    public int FailedCount { get; set; }
}

public class PushCampaignHistoryDto
{
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public int RecipientCount { get; set; }
    public int FailedCount { get; set; }
    public int SentCount { get; set; }
}

public class SendEventPushRequest
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string>? Data { get; set; }
}

public class RecordPushEngagementDto
{
    public string EngagementType { get; set; } = string.Empty;
}

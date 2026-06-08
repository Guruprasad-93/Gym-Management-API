using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Mobile;
using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class MobilePushRepository : IMobilePushRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public MobilePushRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<int> RegisterDeviceAsync(Guid gymId, Guid userId, RegisterDeviceDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", userId);
        parameters.Add("@DeviceType", dto.DeviceType);
        parameters.Add("@DeviceToken", dto.DeviceToken);
        parameters.Add("@AppVersion", dto.AppVersion);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.RegisterDeviceToken, parameters, "@Id", cancellationToken);
    }

    public Task UnregisterDeviceAsync(Guid gymId, Guid userId, string deviceToken, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UnregisterDeviceToken, new { GymId = gymId, UserId = userId, DeviceToken = deviceToken }, cancellationToken);

    public async Task<IReadOnlyList<DeviceTokenDto>> GetActiveDevicesAsync(Guid gymId, Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<DeviceTokenRow>(StoredProcedureNames.GetActiveDeviceTokensForUser, new { GymId = gymId, UserId = userId }, cancellationToken);
        return rows.Select(MapDevice).ToList();
    }

    public async Task<int> CreatePushNotificationAsync(Guid gymId, Guid userId, string title, string message, string notificationType, string? dataJson, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", userId);
        parameters.Add("@Title", title);
        parameters.Add("@Message", message);
        parameters.Add("@NotificationType", notificationType);
        parameters.Add("@DataJson", dataJson);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreatePushNotification, parameters, "@Id", cancellationToken);
    }

    public Task UpdatePushStatusAsync(int id, Guid gymId, string status, string? failureReason, DateTime? sentDate, DateTime? deliveredDate, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdatePushNotificationStatus, new
        {
            Id = id,
            GymId = gymId,
            Status = status,
            FailureReason = failureReason,
            SentDate = sentDate,
            DeliveredDate = deliveredDate
        }, cancellationToken);

    public Task MarkReadAsync(Guid gymId, Guid userId, IReadOnlyList<int>? notificationIds, CancellationToken cancellationToken = default)
    {
        var ids = notificationIds is null || notificationIds.Count == 0
            ? null
            : string.Join(',', notificationIds);
        return _sp.ExecuteAsync(StoredProcedureNames.MarkPushNotificationsRead, new { GymId = gymId, UserId = userId, NotificationIds = ids }, cancellationToken);
    }

    public Task RecordEngagementAsync(int id, Guid gymId, Guid userId, string engagementType, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.RecordPushNotificationEngagement, new { Id = id, GymId = gymId, UserId = userId, EngagementType = engagementType }, cancellationToken);

    public async Task<PagedResultDto<PushNotificationDto>> GetNotificationsPagedAsync(Guid gymId, Guid userId, PushNotificationQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", userId);
        parameters.Add("@UnreadOnly", query.UnreadOnly);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<PushNotificationRow>(StoredProcedureNames.GetPushNotificationsPaged, parameters, cancellationToken);
        var total = parameters.Get<int>("@TotalCount");
        return new PagedResultDto<PushNotificationDto>
        {
            Items = rows.Select(MapPush).ToList(),
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IReadOnlyList<PendingPushRow>> GetPendingPushNotificationsAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<PendingPushRow>(StoredProcedureNames.GetPendingPushNotifications, new { BatchSize = batchSize }, cancellationToken);
        return rows.ToList();
    }

    public async Task<NotificationPreferencesDto> GetOrCreatePreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<NotificationPreferencesRow>(StoredProcedureNames.GetOrCreateNotificationPreferences, new { UserId = userId }, cancellationToken);
        if (row == null)
            throw new InvalidOperationException($"Notification preferences could not be loaded for user {userId}.");
        return MapPreferences(row);
    }

    public Task UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateNotificationPreferences, new
        {
            UserId = userId,
            dto.PushEnabled,
            dto.MembershipReminders,
            dto.WorkoutReminders,
            dto.DietReminders,
            dto.AttendanceReminders,
            dto.PromotionalNotifications
        }, cancellationToken);

    public async Task<bool> IsCategoryEnabledAsync(Guid userId, string notificationType, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@NotificationType", notificationType);
        parameters.Add("@IsEnabled", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.IsPushCategoryEnabled, parameters, cancellationToken);
        return parameters.Get<bool>("@IsEnabled");
    }

    public async Task<MobileDashboardDto> GetMobileDashboardAsync(Guid gymId, Guid userId, int memberId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetMobileDashboard,
            new { GymId = gymId, UserId = userId, MemberId = memberId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var membership = await multi.ReadSingleOrDefaultAsync<MobileMembershipRow>();
        var attendance = await multi.ReadSingleOrDefaultAsync<MobileAttendanceRow>();
        var goal = await multi.ReadSingleOrDefaultAsync<MobileGoalRow>();
        var workout = await multi.ReadSingleOrDefaultAsync<MobileWorkoutRow>();
        var diet = await multi.ReadSingleOrDefaultAsync<MobileDietRow>();
        var water = await multi.ReadSingleOrDefaultAsync<MobileWaterRow>();
        var unread = await multi.ReadSingleOrDefaultAsync<UnreadCountRow>();
        var notifications = (await multi.ReadAsync<PushNotificationRow>()).ToList();

        return new MobileDashboardDto
        {
            Membership = membership is null ? null : new MobileMembershipSummaryDto
            {
                MembershipId = membership.MembershipId,
                PlanName = membership.PlanName,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                Status = membership.Status,
                RemainingDays = membership.RemainingDays
            },
            Attendance = new MobileAttendanceSummaryDto
            {
                TotalDays = attendance?.TotalDays ?? 0,
                PresentDays = attendance?.PresentDays ?? 0
            },
            Goal = goal is null ? null : new MobileGoalSummaryDto
            {
                GoalId = goal.GoalId,
                GoalType = goal.GoalType,
                TargetValue = goal.TargetValue,
                CurrentValue = goal.CurrentValue,
                TargetDate = goal.TargetDate,
                Status = goal.Status,
                ProgressPercent = goal.ProgressPercent
            },
            TodayWorkout = workout is null ? null : new MobileWorkoutTodayDto
            {
                WorkoutTrackingId = workout.WorkoutTrackingId,
                PlanName = workout.PlanName,
                WorkoutDate = workout.WorkoutDate,
                Completed = workout.CompletionPercentage >= 100
            },
            TodayDiet = diet is null ? null : new MobileDietTodayDto
            {
                DietTrackingId = diet.DietTrackingId,
                PlanName = diet.PlanName,
                TrackingDate = diet.TrackingDate,
                Completed = diet.CompliancePercentage >= 100
            },
            WaterToday = water is null ? null : new MobileWaterTodayDto
            {
                WaterIntakeId = water.WaterIntakeId,
                AmountMl = water.ConsumedLitres * 1000,
                LogDate = water.LogDate
            },
            UnreadNotificationCount = unread?.UnreadCount ?? 0,
            RecentNotifications = notifications.Select(MapPush).ToList()
        };
    }

    public async Task<MobileSyncProfileDto?> GetSyncProfileAsync(Guid gymId, Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MobileSyncProfileRow>(StoredProcedureNames.GetMobileSyncProfile, new { GymId = gymId, UserId = userId }, cancellationToken);
        return row is null ? null : new MobileSyncProfileDto
        {
            UserId = row.UserId,
            Name = row.Name,
            Email = row.Email,
            GymId = row.GymId,
            GymName = row.GymName,
            MemberId = row.MemberId,
            Phone = row.Phone,
            BranchId = row.BranchId,
            BranchName = row.BranchName,
            LogoUrl = row.LogoUrl,
            PrimaryColor = row.PrimaryColor,
            SecondaryColor = row.SecondaryColor
        };
    }

    public async Task<MobileSyncDeltaDto> GetSyncDeltaAsync(Guid gymId, Guid userId, int memberId, DateTime lastSyncDate, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetMobileSyncDelta,
            new { GymId = gymId, UserId = userId, MemberId = memberId, LastSyncDate = lastSyncDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var notifications = (await multi.ReadAsync<PushNotificationRow>()).Select(MapPush).ToList();
        var goals = (await multi.ReadAsync<object>()).ToList();
        var water = memberId > 0 ? (await multi.ReadAsync<object>()).ToList() : new List<object>();

        return new MobileSyncDeltaDto
        {
            LastSyncDate = lastSyncDate,
            ServerTimeUtc = DateTime.UtcNow,
            Notifications = notifications,
            UpdatedGoals = goals,
            UpdatedWaterLogs = water
        };
    }

    public async Task<PushNotificationAnalyticsDto> GetAnalyticsAsync(Guid gymId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetPushNotificationAnalytics,
            new { GymId = gymId, FromDate = fromDate, ToDate = toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var summary = await multi.ReadSingleAsync<PushAnalyticsSummaryRow>();
        var byType = (await multi.ReadAsync<PushTypeStatRow>()).ToList();
        return new PushNotificationAnalyticsDto
        {
            TotalSent = summary.TotalSent,
            TotalDelivered = summary.TotalDelivered,
            TotalFailed = summary.TotalFailed,
            TotalOpened = summary.TotalOpened,
            TotalClicked = summary.TotalClicked,
            TotalPending = summary.TotalPending,
            ActiveDevices = summary.ActiveDevices,
            ByType = byType.Select(t => new PushNotificationTypeStatDto
            {
                NotificationType = t.NotificationType,
                Count = t.Count,
                FailedCount = t.FailedCount
            }).ToList()
        };
    }

    public async Task<PagedResultDto<PushCampaignHistoryDto>> GetCampaignHistoryAsync(Guid gymId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<PushCampaignRow>(StoredProcedureNames.SearchPushNotificationCampaigns, parameters, cancellationToken);
        return new PagedResultDto<PushCampaignHistoryDto>
        {
            Items = rows.Select(r => new PushCampaignHistoryDto
            {
                NotificationType = r.NotificationType,
                Title = r.Title,
                Message = r.Message,
                SentDate = r.SentDate,
                RecipientCount = r.RecipientCount,
                FailedCount = r.FailedCount,
                SentCount = r.SentCount
            }).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<MembershipPushCandidateRow>> GetMembershipsExpiringForPushAsync(int daysUntilExpiry, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MembershipPushCandidateRow>(StoredProcedureNames.GetMembershipsExpiringForPush, new { DaysUntilExpiry = daysUntilExpiry }, cancellationToken);
        return rows.ToList();
    }

    public Task<IReadOnlyList<MemberPushCandidateRow>> GetAttendanceReminderCandidatesAsync(CancellationToken cancellationToken = default) =>
        QueryCandidates(StoredProcedureNames.GetMembersForAttendancePushReminder, cancellationToken);

    public Task<IReadOnlyList<MemberPushCandidateRow>> GetWorkoutReminderCandidatesAsync(CancellationToken cancellationToken = default) =>
        QueryCandidates(StoredProcedureNames.GetMembersForWorkoutPushReminder, cancellationToken);

    public Task<IReadOnlyList<MemberPushCandidateRow>> GetDietReminderCandidatesAsync(CancellationToken cancellationToken = default) =>
        QueryCandidates(StoredProcedureNames.GetMembersForDietPushReminder, cancellationToken);

    public async Task<IReadOnlyList<GoalPushCandidateRow>> GetGoalReminderCandidatesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GoalPushCandidateRow>(StoredProcedureNames.GetMembersForGoalPushReminder, cancellationToken: cancellationToken);
        return rows.ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetMemberUserIdsAsync(Guid gymId, int? branchId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<UserIdRow>(StoredProcedureNames.GetMobilePushRecipientUserIds, new { GymId = gymId, BranchId = branchId }, cancellationToken);
        return rows.Select(r => r.UserId).ToList();
    }

    private async Task<IReadOnlyList<MemberPushCandidateRow>> QueryCandidates(string spName, CancellationToken cancellationToken)
    {
        var rows = await _sp.QueryAsync<MemberPushCandidateRow>(spName, cancellationToken: cancellationToken);
        return rows.ToList();
    }

    private static DeviceTokenDto MapDevice(DeviceTokenRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        UserId = row.UserId,
        DeviceType = row.DeviceType,
        DeviceToken = row.DeviceToken,
        AppVersion = row.AppVersion,
        LastActiveDate = row.LastActiveDate,
        IsActive = row.IsActive
    };

    private static PushNotificationDto MapPush(PushNotificationRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        UserId = row.UserId,
        Title = row.Title,
        Message = row.Message,
        NotificationType = row.NotificationType,
        DataJson = row.DataJson,
        Status = row.Status,
        IsRead = row.IsRead,
        SentDate = row.SentDate,
        DeliveredDate = row.DeliveredDate,
        ReadDate = row.ReadDate,
        OpenedDate = row.OpenedDate,
        ClickedDate = row.ClickedDate,
        FailureReason = row.FailureReason,
        CreatedDate = row.CreatedDate
    };

    private static NotificationPreferencesDto MapPreferences(NotificationPreferencesRow row) => new()
    {
        Id = row.Id,
        UserId = row.UserId,
        PushEnabled = row.PushEnabled,
        MembershipReminders = row.MembershipReminders,
        WorkoutReminders = row.WorkoutReminders,
        DietReminders = row.DietReminders,
        AttendanceReminders = row.AttendanceReminders,
        PromotionalNotifications = row.PromotionalNotifications,
        UpdatedDate = row.UpdatedDate
    };

    private sealed class DeviceTokenRow
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

    private sealed class PushNotificationRow
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

    private sealed class NotificationPreferencesRow
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

    private sealed class MobileMembershipRow
    {
        public int MembershipId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int RemainingDays { get; set; }
    }

    private sealed class MobileAttendanceRow
    {
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
    }

    private sealed class MobileGoalRow
    {
        public int GoalId { get; set; }
        public string GoalType { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal CurrentValue { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
    }

    private sealed class MobileWorkoutRow
    {
        public int WorkoutTrackingId { get; set; }
        public string? PlanName { get; set; }
        public DateTime WorkoutDate { get; set; }
        public decimal CompletionPercentage { get; set; }
    }

    private sealed class MobileDietRow
    {
        public int DietTrackingId { get; set; }
        public string? PlanName { get; set; }
        public DateTime TrackingDate { get; set; }
        public decimal CompliancePercentage { get; set; }
    }

    private sealed class MobileWaterRow
    {
        public int WaterIntakeId { get; set; }
        public decimal ConsumedLitres { get; set; }
        public DateTime LogDate { get; set; }
    }

    private sealed class UnreadCountRow { public int UnreadCount { get; set; } }

    private sealed class MobileSyncProfileRow
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid GymId { get; set; }
        public string? GymName { get; set; }
        public int? MemberId { get; set; }
        public string? Phone { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
    }

    private sealed class PushAnalyticsSummaryRow
    {
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        public int TotalPending { get; set; }
        public int ActiveDevices { get; set; }
    }

    private sealed class PushTypeStatRow
    {
        public string NotificationType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int FailedCount { get; set; }
    }

    private sealed class PushCampaignRow
    {
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public int RecipientCount { get; set; }
        public int FailedCount { get; set; }
        public int SentCount { get; set; }
    }

    private sealed class UserIdRow { public Guid UserId { get; set; } }
}

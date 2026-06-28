using System.Text.Json;
using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Mobile;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Services;

public class MobilePushService : IMobilePushService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IMobilePushRepository _repository;
    private readonly IFirebasePushService _firebase;
    private readonly IMemberRepository _memberRepository;
    private readonly IPermissionResolver _permissionResolver;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<MobilePushService> _logger;

    public MobilePushService(
        IMobilePushRepository repository,
        IFirebasePushService firebase,
        IMemberRepository memberRepository,
        IPermissionResolver permissionResolver,
        IAuditService auditService,
        ICurrentUserService currentUser,
        ILogger<MobilePushService> logger)
    {
        _repository = repository;
        _firebase = firebase;
        _memberRepository = memberRepository;
        _permissionResolver = permissionResolver;
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task RegisterDeviceAsync(RegisterDeviceDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.RequireGymId();
        var userId = _currentUser.UserId!.Value;
        await _repository.RegisterDeviceAsync(gymId, userId, dto, cancellationToken);
    }

    public async Task UnregisterDeviceAsync(UnregisterDeviceDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.RequireGymId();
        var userId = _currentUser.UserId!.Value;
        await _repository.UnregisterDeviceAsync(gymId, userId, dto.DeviceToken, cancellationToken);
    }

    public async Task<MobileDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanViewNotifications();
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetMobileDashboardAsync(gymId, _currentUser.UserId!.Value, memberId, cancellationToken);
    }

    public async Task<MobileSyncDto> GetSyncAsync(CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.RequireGymId();
        var userId = _currentUser.UserId!.Value;
        var profile = await _repository.GetSyncProfileAsync(gymId, userId, cancellationToken)
            ?? throw new KeyNotFoundException("User profile not found.");

        var permissions = await _permissionResolver.GetPermissionsForUserAsync(userId, cancellationToken);
        var roles = await _permissionResolver.GetRolesForUserAsync(userId, cancellationToken);

        return new MobileSyncDto
        {
            Profile = profile,
            Permissions = permissions,
            Roles = roles,
            FeatureFlags = new MobileFeatureFlagsDto(),
            Branding = new MobileBrandingDto
            {
                LogoUrl = profile.LogoUrl,
                PrimaryColor = profile.PrimaryColor,
                SecondaryColor = profile.SecondaryColor
            },
            AppSettings = new MobileAppSettingsDto(),
            ServerTimeUtc = DateTime.UtcNow
        };
    }

    public async Task<MobileSyncDeltaDto> GetSyncDeltaAsync(MobileSyncQueryDto query, CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.RequireGymId();
        var userId = _currentUser.UserId!.Value;
        var lastSync = query.LastSyncDate ?? DateTime.UtcNow.AddDays(-1);
        var memberId = 0;
        if (_currentUser.HasRole(RoleNames.Member))
        {
            var member = await _memberRepository.GetByUserIdAsync(userId, cancellationToken);
            memberId = member?.Id ?? 0;
        }

        return await _repository.GetSyncDeltaAsync(gymId, userId, memberId, lastSync, cancellationToken);
    }

    public async Task<PagedResultDto<PushNotificationDto>> GetNotificationsAsync(PushNotificationQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewNotifications();
        var gymId = _currentUser.RequireGymId();
        return await _repository.GetNotificationsPagedAsync(gymId, _currentUser.UserId!.Value, query, cancellationToken);
    }

    public async Task MarkNotificationsReadAsync(MarkNotificationsReadDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanViewNotifications();
        var gymId = _currentUser.RequireGymId();
        await _repository.MarkReadAsync(gymId, _currentUser.UserId!.Value, dto.NotificationIds, cancellationToken);
    }

    public async Task RecordEngagementAsync(int notificationId, RecordPushEngagementDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanViewNotifications();
        var gymId = _currentUser.RequireGymId();
        await _repository.RecordEngagementAsync(notificationId, gymId, _currentUser.UserId!.Value, dto.EngagementType, cancellationToken);
    }

    public async Task<NotificationPreferencesDto> GetPreferencesAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanManagePreferences();
        return await _repository.GetOrCreatePreferencesAsync(_currentUser.UserId!.Value, cancellationToken);
    }

    public async Task UpdatePreferencesAsync(UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManagePreferences();
        await _repository.UpdatePreferencesAsync(_currentUser.UserId!.Value, dto, cancellationToken);
    }

    public async Task SendEventPushAsync(Guid gymId, SendEventPushRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _repository.IsCategoryEnabledAsync(request.UserId, request.NotificationType, cancellationToken))
                return;

            var dataJson = request.Data is null ? null : JsonSerializer.Serialize(request.Data, JsonOptions);
            var id = await _repository.CreatePushNotificationAsync(
                gymId, request.UserId, request.Title, request.Message, request.NotificationType, dataJson, cancellationToken);
            await DispatchPushAsync(id, gymId, request.UserId, request.Title, request.Message, request.Data, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Event push {Type} failed for user {UserId}", request.NotificationType, request.UserId);
        }
    }

    public async Task ProcessPendingPushNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _repository.GetPendingPushNotificationsAsync(100, cancellationToken);
        foreach (var item in pending)
        {
            Dictionary<string, string>? data = null;
            if (!string.IsNullOrWhiteSpace(item.DataJson))
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(item.DataJson, JsonOptions);

            await DispatchPushAsync(item.Id, item.GymId, item.UserId, item.Title, item.Message, data, cancellationToken, item.DeviceToken, item.DeviceType);
        }
    }

    public async Task QueueScheduledRemindersAsync(CancellationToken cancellationToken = default)
    {
        foreach (var days in new[] { (7, PushNotificationTypes.MembershipExpiry7Days), (3, PushNotificationTypes.MembershipExpiry3Days), (0, PushNotificationTypes.MembershipExpiryToday) })
        {
            var candidates = await _repository.GetMembershipsExpiringForPushAsync(days.Item1, cancellationToken);
            foreach (var c in candidates)
            {
                await SendEventPushAsync(c.GymId, new SendEventPushRequest
                {
                    UserId = c.UserId,
                    NotificationType = days.Item2,
                    Title = "Membership Reminder",
                    Message = $"Hi {c.MemberName}, your {c.PlanName} membership expires on {c.EndDate:dd MMM yyyy}."
                }, cancellationToken);
            }
        }

        foreach (var c in await _repository.GetAttendanceReminderCandidatesAsync(cancellationToken))
        {
            await SendEventPushAsync(c.GymId, new SendEventPushRequest
            {
                UserId = c.UserId,
                NotificationType = PushNotificationTypes.AttendanceReminder,
                Title = "Attendance Reminder",
                Message = $"Hi {c.MemberName}, don't forget to check in at the gym today!"
            }, cancellationToken);
        }

        foreach (var c in await _repository.GetCheckoutReminderCandidatesAsync(cancellationToken))
        {
            await SendEventPushAsync(c.GymId, new SendEventPushRequest
            {
                UserId = c.UserId,
                NotificationType = PushNotificationTypes.CheckoutReminder,
                Title = "Check-Out Reminder",
                Message = $"Hi {c.MemberName}, the gym is closing soon. Please check out before you leave."
            }, cancellationToken);
        }

        foreach (var c in await _repository.GetWorkoutReminderCandidatesAsync(cancellationToken))
        {
            await SendEventPushAsync(c.GymId, new SendEventPushRequest
            {
                UserId = c.UserId,
                NotificationType = PushNotificationTypes.WorkoutReminder,
                Title = "Workout Reminder",
                Message = $"Hi {c.MemberName}, your workout plan is waiting for you today."
            }, cancellationToken);
        }

        foreach (var c in await _repository.GetDietReminderCandidatesAsync(cancellationToken))
        {
            await SendEventPushAsync(c.GymId, new SendEventPushRequest
            {
                UserId = c.UserId,
                NotificationType = PushNotificationTypes.DietReminder,
                Title = "Diet Reminder",
                Message = $"Hi {c.MemberName}, log your meals and stay on track with your diet plan."
            }, cancellationToken);
        }

        foreach (var c in await _repository.GetGoalReminderCandidatesAsync(cancellationToken))
        {
            await SendEventPushAsync(c.GymId, new SendEventPushRequest
            {
                UserId = c.UserId,
                NotificationType = PushNotificationTypes.GoalReminder,
                Title = "Goal Deadline Approaching",
                Message = $"Hi {c.MemberName}, your {c.GoalType} goal is due on {c.TargetDate:dd MMM yyyy}."
            }, cancellationToken);
        }
    }

    public async Task SendCampaignAsync(SendPushCampaignDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var audience = string.IsNullOrWhiteSpace(dto.TargetAudience)
            ? PushCampaignAudiences.ActiveMembers
            : dto.TargetAudience.Trim();

        if (string.Equals(audience, PushCampaignAudiences.SelectedMembers, StringComparison.OrdinalIgnoreCase)
            && (dto.UserIds is null || dto.UserIds.Count == 0))
        {
            throw new ArgumentException("Select at least one member for the selected audience.");
        }

        IReadOnlyList<Guid> userIds = string.Equals(audience, PushCampaignAudiences.SelectedMembers, StringComparison.OrdinalIgnoreCase)
            ? await _repository.GetCampaignRecipientUserIdsAsync(
                gymId, audience, dto.BranchId, dto.ExpiringWithinDays, dto.UserIds, cancellationToken)
            : await _repository.GetCampaignRecipientUserIdsAsync(
                gymId, audience, dto.BranchId, dto.ExpiringWithinDays, null, cancellationToken);

        if (userIds.Count == 0)
            throw new ArgumentException("No recipients match the selected audience. Members must have the mobile app installed with a registered device token.");

        foreach (var userId in userIds)
        {
            await SendEventPushAsync(gymId, new SendEventPushRequest
            {
                UserId = userId,
                NotificationType = PushNotificationTypes.ManualCampaign,
                Title = dto.Title,
                Message = dto.Message
            }, cancellationToken);
        }

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.PushNotification,
            EntityId = dto.Title,
            ActionType = AuditActionTypes.Send,
            NewValue = new { dto.Title, dto.Message, dto.TargetAudience, RecipientCount = userIds.Count }
        }, cancellationToken);
    }

    public async Task<PushNotificationAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetAnalyticsAsync(gymId, fromDate, toDate, cancellationToken);
    }

    public Task<PagedResultDto<PushCampaignHistoryDto>> GetCampaignHistoryAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return _repository.GetCampaignHistoryAsync(gymId, pageNumber, pageSize, cancellationToken);
    }

    private async Task DispatchPushAsync(
        int notificationId,
        Guid gymId,
        Guid userId,
        string title,
        string message,
        Dictionary<string, string>? data,
        CancellationToken cancellationToken,
        string? deviceToken = null,
        string? deviceType = null)
    {
        var devices = await _repository.GetActiveDevicesAsync(gymId, userId, cancellationToken);
        if (devices.Count == 0)
        {
            await _repository.UpdatePushStatusAsync(notificationId, gymId, PushNotificationStatuses.Failed, "No active device tokens.", null, null, cancellationToken);
            return;
        }

        var target = devices.FirstOrDefault(d => deviceToken is null || d.DeviceToken == deviceToken) ?? devices[0];
        var result = await _firebase.SendAsync(new FirebasePushMessage
        {
            DeviceToken = target.DeviceToken,
            DeviceType = deviceType ?? target.DeviceType,
            Title = title,
            Body = message,
            Data = data
        }, cancellationToken);

        var now = DateTime.UtcNow;
        if (result.Success)
        {
            await _repository.UpdatePushStatusAsync(notificationId, gymId, PushNotificationStatuses.Delivered, null, now, now, cancellationToken);
        }
        else
        {
            await _repository.UpdatePushStatusAsync(notificationId, gymId, PushNotificationStatuses.Failed, result.ErrorMessage, now, null, cancellationToken);
        }
    }

    private async Task<(Guid GymId, int MemberId)> ResolveCurrentMemberAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.HasRole(RoleNames.Member))
            throw new UnauthorizedAccessException("Member profile required.");

        var member = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
        return (member.GymId, member.Id);
    }

    private void EnsureCanViewNotifications()
    {
        if (!_currentUser.HasPermission(Permissions.ViewMobileNotifications))
            throw new UnauthorizedAccessException("View mobile notifications permission required.");
    }

    private void EnsureCanManagePreferences()
    {
        if (!_currentUser.HasPermission(Permissions.ManageNotificationPreferences))
            throw new UnauthorizedAccessException("Manage notification preferences permission required.");
    }
}

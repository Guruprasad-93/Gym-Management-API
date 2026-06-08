using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class MemberSelfService : IMemberSelfService
{
    private readonly IMemberSelfServiceRepository _repository;
    private readonly IMemberRepository _memberRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly IMobilePushService _mobilePushService;
    private readonly IMemberSelfServiceReportExporter _reportExporter;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly ICurrentUserService _currentUser;

    public MemberSelfService(
        IMemberSelfServiceRepository repository,
        IMemberRepository memberRepository,
        IAttendanceRepository attendanceRepository,
        IAuditService auditService,
        INotificationService notificationService,
        IMobilePushService mobilePushService,
        IMemberSelfServiceReportExporter reportExporter,
        IQrCodeGenerator qrCodeGenerator,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _attendanceRepository = attendanceRepository;
        _auditService = auditService;
        _notificationService = notificationService;
        _mobilePushService = mobilePushService;
        _reportExporter = reportExporter;
        _qrCodeGenerator = qrCodeGenerator;
        _currentUser = currentUser;
    }

    public async Task<MemberSelfServiceDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetDashboardAsync(gymId, memberId, cancellationToken);
    }

    public async Task<MemberSelfServiceAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetAnalyticsAsync(gymId, memberId, cancellationToken);
    }

    public async Task<ProgressTrendDto> GetProgressTrendsAsync(ProgressQueryDto query, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var to = query.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = query.FromDate ?? to.AddDays(-90);
        return new ProgressTrendDto
        {
            Entries = await _repository.GetProgressHistoryAsync(gymId, memberId, from, to, cancellationToken),
            WaterHistory = await _repository.GetWaterIntakeHistoryAsync(gymId, memberId, from, to, cancellationToken),
            WorkoutHistory = await _repository.GetWorkoutHistoryAsync(gymId, memberId, from, to, cancellationToken),
            DietHistory = await _repository.GetDietHistoryAsync(gymId, memberId, from, to, cancellationToken)
        };
    }

    public async Task<MemberGoalDto> CreateGoalAsync(CreateMemberGoalDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var created = await _repository.CreateGoalAsync(gymId, memberId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberGoal, created.GoalId.ToString(), AuditActionTypes.Create, created, cancellationToken);
        return created;
    }

    public async Task UpdateGoalAsync(int goalId, UpdateMemberGoalDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        await _repository.UpdateGoalAsync(goalId, gymId, memberId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberGoal, goalId.ToString(), AuditActionTypes.Update, dto, cancellationToken);
    }

    public async Task UpdateGoalProgressAsync(int goalId, UpdateGoalProgressDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        await _repository.UpdateGoalProgressAsync(goalId, gymId, memberId, dto.CurrentValue, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberGoal, goalId.ToString(), AuditActionTypes.Update, dto, cancellationToken);
    }

    public async Task CompleteGoalAsync(int goalId, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var goal = await _repository.GetGoalByIdAsync(goalId, gymId, memberId, cancellationToken)
            ?? throw new KeyNotFoundException("Goal not found.");
        await _repository.CompleteGoalAsync(goalId, gymId, memberId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberGoal, goalId.ToString(), AuditActionTypes.Update, new { Status = GoalStatuses.Completed }, cancellationToken);
        await SendGoalCompletedNotificationAsync(gymId, memberId, goal, cancellationToken);
    }

    public async Task<IReadOnlyList<MemberGoalDto>> GetGoalsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetGoalsAsync(gymId, memberId, status, cancellationToken);
    }

    public async Task<MemberProgressEntryDto> CreateProgressAsync(CreateMemberProgressDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var created = await _repository.CreateProgressAsync(gymId, memberId, _currentUser.UserId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberProgress, created.ProgressId.ToString(), AuditActionTypes.Create, created, cancellationToken);
        return created;
    }

    public async Task<IReadOnlyList<MemberProgressEntryDto>> GetProgressHistoryAsync(ProgressQueryDto query, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetProgressHistoryAsync(gymId, memberId, query.FromDate, query.ToDate, cancellationToken);
    }

    public async Task<MemberProgressPhotoDto> CreateProgressPhotoAsync(CreateProgressPhotoDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var created = await _repository.CreateProgressPhotoAsync(gymId, memberId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberProgressPhoto, created.ProgressPhotoId.ToString(), AuditActionTypes.Create, created, cancellationToken);
        return created;
    }

    public async Task<IReadOnlyList<MemberProgressPhotoDto>> GetProgressPhotosAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetProgressPhotosAsync(gymId, memberId, cancellationToken);
    }

    public async Task<WaterIntakeDto> UpsertWaterIntakeAsync(UpsertWaterIntakeDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var result = await _repository.UpsertWaterIntakeAsync(gymId, memberId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.WaterIntake, result.WaterIntakeId.ToString(), AuditActionTypes.Update, result, cancellationToken);
        return result;
    }

    public async Task<WaterIntakeDto?> GetTodayWaterIntakeAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _repository.GetWaterIntakeAsync(gymId, memberId, today, cancellationToken);
    }

    public async Task<WorkoutTrackingDto> UpsertWorkoutTrackingAsync(UpsertWorkoutTrackingDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var result = await _repository.UpsertWorkoutTrackingAsync(gymId, memberId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.WorkoutTracking, result.WorkoutTrackingId.ToString(), AuditActionTypes.Update, result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<WorkoutTrackingDto>> GetWorkoutHistoryAsync(ProgressQueryDto query, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var to = query.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = query.FromDate ?? to.AddDays(-30);
        return await _repository.GetWorkoutHistoryAsync(gymId, memberId, from, to, cancellationToken);
    }

    public async Task<int> GetWorkoutStreakAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetWorkoutStreakAsync(gymId, memberId, cancellationToken);
    }

    public async Task<DietTrackingDto> UpsertDietTrackingAsync(UpsertDietTrackingDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var result = await _repository.UpsertDietTrackingAsync(gymId, memberId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.DietTracking, result.DietTrackingId.ToString(), AuditActionTypes.Update, result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<DietTrackingDto>> GetDietHistoryAsync(ProgressQueryDto query, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var to = query.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = query.FromDate ?? to.AddDays(-30);
        return await _repository.GetDietHistoryAsync(gymId, memberId, from, to, cancellationToken);
    }

    public async Task<DietComplianceSummaryDto> GetDietComplianceAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetDietComplianceAsync(gymId, memberId, cancellationToken);
    }

    public async Task<ReferralStatsDto> GetReferralsAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetReferralStatsAsync(gymId, memberId, cancellationToken);
    }

    public async Task<MemberFeedbackDto> SubmitFeedbackAsync(CreateMemberFeedbackDto dto, CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var created = await _repository.CreateFeedbackAsync(gymId, memberId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberFeedback, created.FeedbackId.ToString(), AuditActionTypes.Create, created, cancellationToken);
        return created;
    }

    public async Task<IReadOnlyList<MemberFeedbackDto>> GetFeedbackAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        return await _repository.GetFeedbackAsync(gymId, memberId, cancellationToken);
    }

    public async Task<MemberQrCodeDto> GetQrCodeAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var token = await _repository.GetOrCreateQrTokenAsync(gymId, memberId, cancellationToken);
        var payload = $"GMS:{gymId}:{memberId}:{token}";
        return new MemberQrCodeDto
        {
            MemberId = memberId,
            Payload = payload,
            QrCodeBase64 = _qrCodeGenerator.GenerateBase64Png(payload)
        };
    }

    public async Task<int> ScanQrCheckInAsync(QrCheckInDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.ManageAttendance))
            throw new UnauthorizedAccessException("Attendance management permission required.");

        var gymId = _currentUser.RequireGymId();
        var (payloadGymId, memberId, token) = ParseQrPayload(dto.QrPayload);
        if (payloadGymId != gymId)
            throw new UnauthorizedAccessException("QR code gym mismatch.");

        var attendanceId = await _repository.QrCheckInAsync(gymId, memberId, token, _currentUser.UserId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.MemberAttendance, attendanceId.ToString(), AuditActionTypes.CheckIn, new { QrCheckIn = true, memberId }, cancellationToken);
        return attendanceId;
    }

    public async Task<byte[]> ExportProgressPdfAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var member = await _memberRepository.GetByIdAsync(memberId, gymId, null, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
        var entries = await _repository.GetProgressHistoryAsync(gymId, memberId, null, null, cancellationToken);
        var bytes = _reportExporter.ExportProgressPdf(member.FullName, entries);
        await LogAsync(gymId, AuditEntityNames.MemberProgress, memberId.ToString(), AuditActionTypes.Export, null, cancellationToken);
        return bytes;
    }

    public async Task<byte[]> ExportAttendancePdfAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var member = await _memberRepository.GetByIdAsync(memberId, gymId, null, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
        var history = await _attendanceRepository.GetMemberHistoryAsync(
            gymId, null, memberId,
            new AttendanceQueryDto { PageNumber = 1, PageSize = 500, SortColumn = "AttendanceDate", SortDirection = "desc" },
            cancellationToken);
        var bytes = _reportExporter.ExportAttendancePdf(member.FullName, history.Items);
        await LogAsync(gymId, AuditEntityNames.MemberAttendance, memberId.ToString(), AuditActionTypes.Export, null, cancellationToken);
        return bytes;
    }

    public async Task<byte[]> ExportGoalSummaryPdfAsync(CancellationToken cancellationToken = default)
    {
        var (gymId, memberId) = await ResolveCurrentMemberAsync(cancellationToken);
        var member = await _memberRepository.GetByIdAsync(memberId, gymId, null, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
        var goals = await _repository.GetGoalsAsync(gymId, memberId, null, cancellationToken);
        var bytes = _reportExporter.ExportGoalSummaryPdf(member.FullName, goals);
        await LogAsync(gymId, AuditEntityNames.MemberGoal, memberId.ToString(), AuditActionTypes.Export, null, cancellationToken);
        return bytes;
    }

    private async Task<(Guid GymId, int MemberId)> ResolveCurrentMemberAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.HasRole(RoleNames.Member))
            throw new UnauthorizedAccessException("Member profile required.");

        var member = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
        return (member.GymId, member.Id);
    }

    private async Task SendGoalCompletedNotificationAsync(Guid gymId, int memberId, MemberGoalDto goal, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(memberId, gymId, null, cancellationToken);
        if (member?.Phone is null) return;

        await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
        {
            NotificationType = NotificationTypes.GoalCompleted,
            PhoneNumber = member.Phone,
            RecipientUserId = member.UserId,
            MemberId = memberId,
            Variables = new Dictionary<string, string>
            {
                ["memberName"] = member.FullName,
                ["goalType"] = goal.GoalType
            },
            RelatedEntityType = AuditEntityNames.MemberGoal,
            RelatedEntityId = goal.GoalId.ToString()
        }, cancellationToken);

        await _mobilePushService.SendEventPushAsync(gymId, new DTOs.Mobile.SendEventPushRequest
        {
            UserId = member.UserId,
            NotificationType = PushNotificationTypes.GoalCompleted,
            Title = "Goal Completed!",
            Message = $"Congratulations {member.FullName}! You completed your {goal.GoalType} goal."
        }, cancellationToken);
    }

    private static (Guid GymId, int MemberId, string Token) ParseQrPayload(string payload)
    {
        if (!payload.StartsWith("GMS:", StringComparison.Ordinal))
            throw new ArgumentException("Invalid QR code format.");

        var parts = payload.Split(':');
        if (parts.Length != 4 || !Guid.TryParse(parts[1], out var gymId) || !int.TryParse(parts[2], out var memberId))
            throw new ArgumentException("Invalid QR code format.");

        return (gymId, memberId, parts[3]);
    }

    private Task LogAsync(Guid gymId, string entity, string entityId, string action, object? value, CancellationToken cancellationToken) =>
        _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = entity,
            EntityId = entityId,
            ActionType = action,
            NewValue = value
        }, cancellationToken);
}

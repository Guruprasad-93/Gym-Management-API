using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IMemberRepository memberRepository,
        ITrainerRepository trainerRepository,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _attendanceRepository = attendanceRepository;
        _memberRepository = memberRepository;
        _trainerRepository = trainerRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<AttendanceStatusDto>> GetStatusesAsync(CancellationToken cancellationToken = default) =>
        _attendanceRepository.GetStatusesAsync(cancellationToken);

    public async Task<MemberAttendanceDto> CheckInAsync(CheckInMemberDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForMemberAsync(dto.MemberId, cancellationToken);
        var result = await _attendanceRepository.CheckInMemberAsync(
            gymId, dto.MemberId, dto.TrainerId, _currentUser.UserId, dto.Notes, cancellationToken);
        await LogAttendanceAsync(gymId, AuditEntityNames.MemberAttendance, result.MemberAttendanceId.ToString(), AuditActionTypes.CheckIn, result, cancellationToken);
        return result;
    }

    public async Task<MemberAttendanceDto> CheckOutAsync(CheckOutMemberDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForMemberAsync(dto.MemberId, cancellationToken);
        var result = await _attendanceRepository.CheckOutMemberAsync(
            gymId, dto.MemberId, _currentUser.UserId, dto.Notes, cancellationToken);
        await LogAttendanceAsync(gymId, AuditEntityNames.MemberAttendance, result.MemberAttendanceId.ToString(), AuditActionTypes.CheckOut, result, cancellationToken);
        return result;
    }

    public async Task<MemberAttendanceDto> MarkAsync(MarkAttendanceDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForMemberAsync(dto.MemberId, cancellationToken);
        var result = await _attendanceRepository.MarkMemberAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        await LogAttendanceAsync(gymId, AuditEntityNames.MemberAttendance, result.MemberAttendanceId.ToString(), AuditActionTypes.Mark, result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<MemberAttendanceDto>> GetTodayAsync(string? search, CancellationToken cancellationToken = default) =>
        await _attendanceRepository.GetTodayAsync(
            ResolveGymScope(), await ResolveTrainerFilterAsync(cancellationToken), search, cancellationToken);

    public async Task<PagedResultDto<MemberAttendanceDto>> GetByDateRangeAsync(AttendanceQueryDto query, CancellationToken cancellationToken = default) =>
        await _attendanceRepository.GetByDateRangeAsync(
            ResolveGymScope(), await ResolveTrainerFilterAsync(cancellationToken), query, cancellationToken);

    public async Task<PagedResultDto<MemberAttendanceDto>> GetMemberHistoryAsync(
        int memberId, AttendanceQueryDto query, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAccessAsync(memberId, cancellationToken);
        return await _attendanceRepository.GetMemberHistoryAsync(
            ResolveGymScope(), await ResolveTrainerFilterAsync(cancellationToken), memberId, query, cancellationToken);
    }

    public async Task<AttendanceDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        await _attendanceRepository.GetDashboardAsync(
            ResolveGymScope(), await ResolveTrainerFilterAsync(cancellationToken), cancellationToken);

    public async Task<DailyAttendanceReportDto> GetDailyReportAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        await _attendanceRepository.GetDailyReportAsync(
            ResolveGymScope(), await ResolveTrainerFilterAsync(cancellationToken), date, cancellationToken);

    public async Task<MonthlyAttendanceReportDto> GetMonthlyReportAsync(int year, int month, CancellationToken cancellationToken = default) =>
        await _attendanceRepository.GetMonthlyReportAsync(
            ResolveGymScope(), await ResolveTrainerFilterAsync(cancellationToken), year, month, cancellationToken);

    public async Task<TrainerAttendanceDto> TrainerCheckInAsync(TrainerCheckInDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForTrainerAsync(dto.TrainerId, cancellationToken);
        var result = await _attendanceRepository.CheckInTrainerAsync(gymId, dto.TrainerId, _currentUser.UserId, dto.Notes, cancellationToken);
        await LogAttendanceAsync(gymId, AuditEntityNames.TrainerAttendance, result.TrainerAttendanceId.ToString(), AuditActionTypes.CheckIn, result, cancellationToken);
        return result;
    }

    public async Task<TrainerAttendanceDto> TrainerCheckOutAsync(int trainerId, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForTrainerAsync(trainerId, cancellationToken);
        var result = await _attendanceRepository.CheckOutTrainerAsync(gymId, trainerId, _currentUser.UserId, cancellationToken);
        await LogAttendanceAsync(gymId, AuditEntityNames.TrainerAttendance, result.TrainerAttendanceId.ToString(), AuditActionTypes.CheckOut, result, cancellationToken);
        return result;
    }

    public async Task<PagedResultDto<TrainerAttendanceDto>> GetTrainerAttendanceAsync(
        int? trainerId, DateOnly from, DateOnly to, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filterTrainer = await ResolveTrainerFilterAsync(cancellationToken);
        if (filterTrainer.HasValue)
            trainerId = filterTrainer;

        return await _attendanceRepository.GetTrainerAttendanceAsync(
            ResolveGymScope(), trainerId, from, to, pageNumber, pageSize, cancellationToken);
    }

    private Task LogAttendanceAsync(Guid gymId, string entityName, string entityId, string actionType, object newValue, CancellationToken ct) =>
        _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = entityName,
            EntityId = entityId,
            ActionType = actionType,
            NewValue = newValue
        }, ct);

    private async Task<Guid> ResolveGymIdForMemberAsync(int memberId, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(memberId, ResolveGymScope(), await ResolveTrainerFilterAsync(cancellationToken), cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
        if (!_currentUser.HasRole(RoleNames.SuperAdmin) && member.GymId != _currentUser.RequireGymId())
            throw new KeyNotFoundException("Member not found.");
        return member.GymId;
    }

    private async Task EnsureMemberAccessAsync(int memberId, CancellationToken cancellationToken)
    {
        _ = await ResolveGymIdForMemberAsync(memberId, cancellationToken);
    }

    private async Task<Guid> ResolveGymIdForTrainerAsync(int trainerId, CancellationToken cancellationToken)
    {
        var trainer = await _trainerRepository.GetByIdAsync(trainerId, ResolveGymScope(), cancellationToken)
            ?? throw new KeyNotFoundException("Trainer not found.");
        return trainer.GymId;
    }

    private Guid ResolveGymScope(Guid? requestedGymId = null) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);

    private async Task<int?> ResolveTrainerFilterAsync(CancellationToken cancellationToken)
    {
        if (!IsTrainerOnly()) return null;
        return await GetTrainerIdForCurrentUserAsync(cancellationToken);
    }

    private bool IsTrainerOnly() =>
        _currentUser.HasRole(RoleNames.Trainer)
        && !_currentUser.HasRole(RoleNames.GymAdmin)
        && !_currentUser.HasRole(RoleNames.SuperAdmin);

    private async Task<int?> GetTrainerIdForCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return null;
        var trainer = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        return trainer?.Id;
    }
}

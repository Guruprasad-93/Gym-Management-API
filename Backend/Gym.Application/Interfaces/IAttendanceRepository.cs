using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.Common;

namespace Gym.Application.Interfaces;

public interface IAttendanceRepository
{
    Task<IReadOnlyList<AttendanceStatusDto>> GetStatusesAsync(CancellationToken cancellationToken = default);

    Task<MemberAttendanceDto> CheckInMemberAsync(
        Guid gymId, int memberId, int? trainerId, Guid? markedByUserId, string? notes,
        CancellationToken cancellationToken = default);

    Task<MemberAttendanceDto> CheckOutMemberAsync(
        Guid gymId, int memberId, Guid? markedByUserId, string? notes,
        CancellationToken cancellationToken = default);

    Task<MemberAttendanceDto> MarkMemberAsync(
        Guid gymId, MarkAttendanceDto dto, Guid? markedByUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemberAttendanceDto>> GetTodayAsync(
        Guid? gymId, int? trainerId, string? search,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<MemberAttendanceDto>> GetByDateRangeAsync(
        Guid? gymId, int? trainerId, AttendanceQueryDto query,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<MemberAttendanceDto>> GetMemberHistoryAsync(
        Guid? gymId, int? trainerId, int memberId, AttendanceQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AttendanceDashboardDto> GetDashboardAsync(
        Guid? gymId, int? trainerId, CancellationToken cancellationToken = default);

    Task<DailyAttendanceReportDto> GetDailyReportAsync(
        Guid? gymId, int? trainerId, DateOnly reportDate,
        CancellationToken cancellationToken = default);

    Task<MonthlyAttendanceReportDto> GetMonthlyReportAsync(
        Guid? gymId, int? trainerId, int year, int month,
        CancellationToken cancellationToken = default);

    Task<TrainerAttendanceDto> CheckInTrainerAsync(
        Guid gymId, int trainerId, Guid? markedByUserId, string? notes,
        CancellationToken cancellationToken = default);

    Task<TrainerAttendanceDto> CheckOutTrainerAsync(
        Guid gymId, int trainerId, Guid? markedByUserId,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<TrainerAttendanceDto>> GetTrainerAttendanceAsync(
        Guid? gymId, int? trainerId, DateOnly from, DateOnly to, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);
}

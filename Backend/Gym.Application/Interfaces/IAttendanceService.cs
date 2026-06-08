using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.Common;

namespace Gym.Application.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceStatusDto>> GetStatusesAsync(CancellationToken cancellationToken = default);
    Task<MemberAttendanceDto> CheckInAsync(CheckInMemberDto dto, CancellationToken cancellationToken = default);
    Task<MemberAttendanceDto> CheckOutAsync(CheckOutMemberDto dto, CancellationToken cancellationToken = default);
    Task<MemberAttendanceDto> MarkAsync(MarkAttendanceDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberAttendanceDto>> GetTodayAsync(string? search, CancellationToken cancellationToken = default);
    Task<PagedResultDto<MemberAttendanceDto>> GetByDateRangeAsync(AttendanceQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<MemberAttendanceDto>> GetMemberHistoryAsync(int memberId, AttendanceQueryDto query, CancellationToken cancellationToken = default);
    Task<AttendanceDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<DailyAttendanceReportDto> GetDailyReportAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<MonthlyAttendanceReportDto> GetMonthlyReportAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<TrainerAttendanceDto> TrainerCheckInAsync(TrainerCheckInDto dto, CancellationToken cancellationToken = default);
    Task<TrainerAttendanceDto> TrainerCheckOutAsync(int trainerId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<TrainerAttendanceDto>> GetTrainerAttendanceAsync(int? trainerId, DateOnly from, DateOnly to, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}

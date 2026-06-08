namespace Gym.Application.DTOs.Attendance;

public class AttendanceStatusDto
{
    public int AttendanceStatusId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class MemberAttendanceDto
{
    public int MemberAttendanceId { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string? MemberEmail { get; set; }
    public int? TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public int AttendanceStatusId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateOnly AttendanceDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TrainerAttendanceDto
{
    public int TrainerAttendanceId { get; set; }
    public Guid GymId { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int AttendanceStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateOnly AttendanceDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CheckInMemberDto
{
    public int MemberId { get; set; }
    public int? TrainerId { get; set; }
    public string? Notes { get; set; }
}

public class CheckOutMemberDto
{
    public int MemberId { get; set; }
    public string? Notes { get; set; }
}

public class MarkAttendanceDto
{
    public int MemberId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public int AttendanceStatusId { get; set; }
    public int? TrainerId { get; set; }
    public string? Notes { get; set; }
}

public class AttendanceQueryDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? MemberId { get; set; }
    public int? StatusId { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortColumn { get; set; } = "CheckInAt";
    public string SortDirection { get; set; } = "desc";
}

public class AttendanceDashboardDto
{
    public int TotalActiveMembers { get; set; }
    public int MembersPresentToday { get; set; }
    public int CurrentlyCheckedIn { get; set; }
    public int AbsentToday { get; set; }
}

public class DailyAttendanceStatusCountDto
{
    public DateOnly ReportDate { get; set; }
    public int AttendanceStatusId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public int RecordCount { get; set; }
}

public class DailyAttendanceDetailDto
{
    public int MemberAttendanceId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
}

public class DailyAttendanceReportDto
{
    public DateOnly ReportDate { get; set; }
    public IReadOnlyList<DailyAttendanceStatusCountDto> StatusCounts { get; set; } = [];
    public IReadOnlyList<DailyAttendanceDetailDto> Details { get; set; } = [];
}

public class MonthlyMemberAttendanceDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int ExcusedDays { get; set; }
    public int TotalRecords { get; set; }
}

public class MonthlyAttendanceReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public IReadOnlyList<MonthlyMemberAttendanceDto> Members { get; set; } = [];
}

public class TrainerCheckInDto
{
    public int TrainerId { get; set; }
    public string? Notes { get; set; }
}

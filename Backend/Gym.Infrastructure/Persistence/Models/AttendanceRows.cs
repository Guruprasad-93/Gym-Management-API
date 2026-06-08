namespace Gym.Infrastructure.Persistence.Models;

internal class MemberAttendanceRow
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
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

internal class TrainerAttendanceRow
{
    public int TrainerAttendanceId { get; set; }
    public Guid GymId { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int AttendanceStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

internal class AttendanceDashboardRow
{
    public int TotalActiveMembers { get; set; }
    public int MembersPresentToday { get; set; }
    public int CurrentlyCheckedIn { get; set; }
    public int AbsentToday { get; set; }
}

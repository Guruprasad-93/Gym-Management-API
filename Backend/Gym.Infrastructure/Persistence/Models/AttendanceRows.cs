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
    public string? CheckoutType { get; set; }
    public bool IsAutoCheckout { get; set; }
    public string? MarkedByName { get; set; }
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

internal class AttendanceSettingsRow
{
    public Guid GymId { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool AutoCheckoutEnabled { get; set; }
    public bool UseClosingTimeForAutoCheckout { get; set; }
    public int CheckoutReminderMinutesBefore { get; set; }
    public string TimeZoneId { get; set; } = string.Empty;
    public bool Is24Hours { get; set; }
    public int MaximumSessionHours { get; set; }
}

internal class MemberTodayVisitRow
{
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string? CheckoutType { get; set; }
    public bool IsAutoCheckout { get; set; }
    public bool IsCurrentlyCheckedIn { get; set; }
    public string? CheckedOutByName { get; set; }
}

internal class AttendanceDashboardRow
{
    public int TotalActiveMembers { get; set; }
    public int MembersPresentToday { get; set; }
    public int CurrentlyCheckedIn { get; set; }
    public int AbsentToday { get; set; }
    public int CheckedOutToday { get; set; }
    public int AutoCheckedOutToday { get; set; }
    public int ManualCheckOutToday { get; set; }
}

internal class ForgotCheckOutReportRow
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int TotalAutoCheckOutCount { get; set; }
    public DateTime? LastAutoCheckOutAt { get; set; }
    public DateTime? LastAutoCheckOutDate { get; set; }
}

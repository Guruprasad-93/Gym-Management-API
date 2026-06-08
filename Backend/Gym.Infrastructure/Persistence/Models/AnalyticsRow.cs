namespace Gym.Infrastructure.Persistence.Models;

internal sealed class AnalyticsOverviewRow
{
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int ExpiringMemberships { get; set; }
    public int ActiveTrainers { get; set; }
}

internal sealed class RevenueSummaryRow
{
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueThisYear { get; set; }
    public int FailedPaymentsCount { get; set; }
}

internal sealed class NamedValueRow
{
    public string? PlanName { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Name { get; set; }
    public decimal Revenue { get; set; }
    public int PaymentCount { get; set; }
}

internal sealed class MembershipSummaryRow
{
    public int ActiveMembers { get; set; }
    public int ExpiredMembers { get; set; }
    public int ExpiringIn7Days { get; set; }
    public int NewRegistrationsThisMonth { get; set; }
    public int ActiveMemberships { get; set; }
}

internal sealed class GrowthRow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int NewMembers { get; set; }
}

internal sealed class NamedCountRow
{
    public string? PlanName { get; set; }
    public string? MonthLabel { get; set; }
    public string? Name { get; set; }
    public int MemberCount { get; set; }
    public int AttendanceCount { get; set; }
}

internal sealed class AttendanceSummaryRow
{
    public int TodayAttendanceCount { get; set; }
    public int UniqueMembersToday { get; set; }
}

internal sealed class WeeklyAttendanceRow
{
    public DateTime AttendanceDate { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public int AttendanceCount { get; set; }
}

internal sealed class MemberActivityRow
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int AttendanceCount { get; set; }
}

internal sealed class MemberPctRow
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal AttendancePercentage { get; set; }
}

internal sealed class TrainerSummaryRow
{
    public int ActiveTrainers { get; set; }
    public int AssignedMembers { get; set; }
}

internal sealed class TrainerPerfRow
{
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int AssignedMembers { get; set; }
    public int TodayAttendance { get; set; }
}

internal sealed class WorkoutSummaryRow
{
    public int ActiveWorkoutPlans { get; set; }
    public int CompletedWorkoutPlans { get; set; }
    public decimal CompletionPercentage { get; set; }
}

internal sealed class DietSummaryRow
{
    public int ActiveDietPlans { get; set; }
    public decimal CompliancePercentage { get; set; }
}

internal sealed class RecentPaymentRow
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? MemberName { get; set; }
}

internal sealed class ExpiringRow
{
    public int MembershipId { get; set; }
    public DateTime EndDate { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
}

internal sealed class NewMemberRow
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
}

internal sealed class RecentNotifRow
{
    public long LogId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

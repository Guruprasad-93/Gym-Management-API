namespace Gym.Application.DTOs.Analytics;

public class AnalyticsDashboardDto
{
    public AnalyticsOverviewDto Overview { get; set; } = new();
    public RevenueAnalyticsDto Revenue { get; set; } = new();
    public MembershipAnalyticsDto Membership { get; set; } = new();
    public AttendanceAnalyticsDto Attendance { get; set; } = new();
    public TrainerAnalyticsDto Trainers { get; set; } = new();
    public WorkoutAnalyticsDto Workouts { get; set; } = new();
    public DietAnalyticsDto Diets { get; set; } = new();
    public AnalyticsWidgetsDto Widgets { get; set; } = new();
}

public class AnalyticsOverviewDto
{
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int ExpiringMemberships { get; set; }
    public int ActiveTrainers { get; set; }
}

public class RevenueAnalyticsDto
{
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueThisYear { get; set; }
    public int FailedPaymentsCount { get; set; }
    public IReadOnlyList<TrendPointDto> RevenueTrend { get; set; } = Array.Empty<TrendPointDto>();
    public IReadOnlyList<NamedValueDto> RevenueByPlan { get; set; } = Array.Empty<NamedValueDto>();
    public IReadOnlyList<NamedValueDto> RevenueByPaymentMethod { get; set; } = Array.Empty<NamedValueDto>();
}

public class MembershipAnalyticsDto
{
    public int ActiveMembers { get; set; }
    public int ExpiredMembers { get; set; }
    public int ExpiringIn7Days { get; set; }
    public int NewRegistrationsThisMonth { get; set; }
    public int ActiveMemberships { get; set; }
    public IReadOnlyList<GrowthPointDto> GrowthTrend { get; set; } = Array.Empty<GrowthPointDto>();
    public IReadOnlyList<NamedCountDto> PlanDistribution { get; set; } = Array.Empty<NamedCountDto>();
}

public class AttendanceAnalyticsDto
{
    public int TodayAttendanceCount { get; set; }
    public int UniqueMembersToday { get; set; }
    public IReadOnlyList<AttendanceTrendPointDto> WeeklyTrend { get; set; } = Array.Empty<AttendanceTrendPointDto>();
    public IReadOnlyList<NamedCountDto> MonthlyTrend { get; set; } = Array.Empty<NamedCountDto>();
    public IReadOnlyList<MemberActivityDto> MostActiveMembers { get; set; } = Array.Empty<MemberActivityDto>();
    public IReadOnlyList<MemberActivityDto> LeastActiveMembers { get; set; } = Array.Empty<MemberActivityDto>();
    public IReadOnlyList<MemberAttendancePercentDto> AttendancePercentage { get; set; } = Array.Empty<MemberAttendancePercentDto>();
}

public class TrainerAnalyticsDto
{
    public int ActiveTrainers { get; set; }
    public int AssignedMembers { get; set; }
    public IReadOnlyList<TrainerPerformanceDto> Performance { get; set; } = Array.Empty<TrainerPerformanceDto>();
}

public class WorkoutAnalyticsDto
{
    public int ActiveWorkoutPlans { get; set; }
    public int CompletedWorkoutPlans { get; set; }
    public decimal CompletionPercentage { get; set; }
}

public class DietAnalyticsDto
{
    public int ActiveDietPlans { get; set; }
    public decimal CompliancePercentage { get; set; }
}

public class AnalyticsWidgetsDto
{
    public IReadOnlyList<RecentPaymentWidgetDto> RecentPayments { get; set; } = Array.Empty<RecentPaymentWidgetDto>();
    public IReadOnlyList<ExpiringMembershipWidgetDto> ExpiringMemberships { get; set; } = Array.Empty<ExpiringMembershipWidgetDto>();
    public IReadOnlyList<NewMemberWidgetDto> NewMembers { get; set; } = Array.Empty<NewMemberWidgetDto>();
    public IReadOnlyList<RecentNotificationWidgetDto> RecentNotifications { get; set; } = Array.Empty<RecentNotificationWidgetDto>();
}

public class TrendPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class GrowthPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int NewMembers { get; set; }
}

public class NamedValueDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
}

public class NamedCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AttendanceTrendPointDto
{
    public DateOnly Date { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MemberActivityDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int AttendanceCount { get; set; }
}

public class MemberAttendancePercentDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal AttendancePercentage { get; set; }
}

public class TrainerPerformanceDto
{
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int AssignedMembers { get; set; }
    public int TodayAttendance { get; set; }
}

public class RecentPaymentWidgetDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? MemberName { get; set; }
}

public class ExpiringMembershipWidgetDto
{
    public int MembershipId { get; set; }
    public DateOnly EndDate { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
}

public class NewMemberWidgetDto
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public DateOnly JoinDate { get; set; }
}

public class RecentNotificationWidgetDto
{
    public long LogId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AnalyticsExportRequestDto
{
    public string ReportType { get; set; } = "dashboard";
}

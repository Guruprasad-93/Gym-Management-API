using Gym.Application.DTOs.Analytics;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public AnalyticsRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<AnalyticsOverviewDto> GetOverviewAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<AnalyticsOverviewRow>(
            StoredProcedureNames.GetAnalyticsDashboardOverview, new { GymId = gymId }, cancellationToken);
        return MapOverview(row);
    }

    public async Task<RevenueAnalyticsDto> GetRevenueAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var summary = await _sp.QuerySingleOrDefaultAsync<RevenueSummaryRow>(
            StoredProcedureNames.GetAnalyticsRevenueSummary, new { GymId = gymId }, cancellationToken);
        var trend = await _sp.QueryAsync<MonthlyRevenueRow>(
            StoredProcedureNames.GetMonthlyRevenueSummary, new { GymId = gymId, Months = 12 }, cancellationToken);
        var byPlan = await _sp.QueryAsync<NamedValueRow>(
            StoredProcedureNames.GetAnalyticsRevenueByPlan, new { GymId = gymId }, cancellationToken);
        var byMethod = await _sp.QueryAsync<NamedValueRow>(
            StoredProcedureNames.GetAnalyticsRevenueByPaymentMethod, new { GymId = gymId }, cancellationToken);

        return new RevenueAnalyticsDto
        {
            RevenueToday = summary?.RevenueToday ?? 0,
            RevenueThisWeek = summary?.RevenueThisWeek ?? 0,
            RevenueThisMonth = summary?.RevenueThisMonth ?? 0,
            RevenueThisYear = summary?.RevenueThisYear ?? 0,
            FailedPaymentsCount = summary?.FailedPaymentsCount ?? 0,
            RevenueTrend = trend.Select(r => new TrendPointDto
            {
                Year = r.Year, Month = r.Month, MonthLabel = r.MonthLabel, Value = r.Revenue
            }).Reverse().ToList(),
            RevenueByPlan = byPlan.Select(r => new NamedValueDto { Name = r.PlanName ?? r.Name ?? "", Value = r.Revenue, Count = r.PaymentCount }).ToList(),
            RevenueByPaymentMethod = byMethod.Select(r => new NamedValueDto { Name = r.PaymentMethod ?? r.Name ?? "", Value = r.Revenue, Count = r.PaymentCount }).ToList()
        };
    }

    public async Task<MembershipAnalyticsDto> GetMembershipAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var summary = await _sp.QuerySingleOrDefaultAsync<MembershipSummaryRow>(
            StoredProcedureNames.GetAnalyticsMembershipSummary, new { GymId = gymId }, cancellationToken);
        var growth = await _sp.QueryAsync<GrowthRow>(
            StoredProcedureNames.GetAnalyticsMembershipGrowthTrend, new { GymId = gymId, Months = 12 }, cancellationToken);
        var plans = await _sp.QueryAsync<NamedCountRow>(
            StoredProcedureNames.GetAnalyticsPlanDistribution, new { GymId = gymId }, cancellationToken);

        return new MembershipAnalyticsDto
        {
            ActiveMembers = summary?.ActiveMembers ?? 0,
            ExpiredMembers = summary?.ExpiredMembers ?? 0,
            ExpiringIn7Days = summary?.ExpiringIn7Days ?? 0,
            NewRegistrationsThisMonth = summary?.NewRegistrationsThisMonth ?? 0,
            ActiveMemberships = summary?.ActiveMemberships ?? 0,
            GrowthTrend = growth.Select(r => new GrowthPointDto { Year = r.Year, Month = r.Month, MonthLabel = r.MonthLabel, NewMembers = r.NewMembers }).Reverse().ToList(),
            PlanDistribution = plans.Select(r => new NamedCountDto { Name = r.PlanName ?? r.Name ?? "", Count = r.MemberCount }).ToList()
        };
    }

    public async Task<AttendanceAnalyticsDto> GetAttendanceAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var summary = await _sp.QuerySingleOrDefaultAsync<AttendanceSummaryRow>(
            StoredProcedureNames.GetAnalyticsAttendanceSummary, new { GymId = gymId }, cancellationToken);
        var weekly = await _sp.QueryAsync<WeeklyAttendanceRow>(
            StoredProcedureNames.GetAnalyticsAttendanceWeeklyTrend, new { GymId = gymId }, cancellationToken);
        var monthly = await _sp.QueryAsync<NamedCountRow>(
            StoredProcedureNames.GetAnalyticsAttendanceMonthlyTrend, new { GymId = gymId, Months = 6 }, cancellationToken);
        var most = await _sp.QueryAsync<MemberActivityRow>(
            StoredProcedureNames.GetAnalyticsMostActiveMembers, new { GymId = gymId, TopN = 10 }, cancellationToken);
        var least = await _sp.QueryAsync<MemberActivityRow>(
            StoredProcedureNames.GetAnalyticsLeastActiveMembers, new { GymId = gymId, TopN = 10 }, cancellationToken);
        var pct = await _sp.QueryAsync<MemberPctRow>(
            StoredProcedureNames.GetAnalyticsMemberAttendancePercentage, new { GymId = gymId }, cancellationToken);

        return new AttendanceAnalyticsDto
        {
            TodayAttendanceCount = summary?.TodayAttendanceCount ?? 0,
            UniqueMembersToday = summary?.UniqueMembersToday ?? 0,
            WeeklyTrend = weekly.Select(r => new AttendanceTrendPointDto { Date = DateOnly.FromDateTime(r.AttendanceDate), DayLabel = r.DayLabel, Count = r.AttendanceCount }).ToList(),
            MonthlyTrend = monthly.Select(r => new NamedCountDto { Name = r.MonthLabel ?? r.Name ?? "", Count = r.AttendanceCount }).ToList(),
            MostActiveMembers = most.Select(MapActivity).ToList(),
            LeastActiveMembers = least.Select(MapActivity).ToList(),
            AttendancePercentage = pct.Select(r => new MemberAttendancePercentDto { MemberId = r.MemberId, MemberName = r.MemberName, AttendancePercentage = r.AttendancePercentage }).ToList()
        };
    }

    public async Task<TrainerAnalyticsDto> GetTrainersAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var summary = await _sp.QuerySingleOrDefaultAsync<TrainerSummaryRow>(
            StoredProcedureNames.GetAnalyticsTrainerSummary, new { GymId = gymId }, cancellationToken);
        var perf = await _sp.QueryAsync<TrainerPerfRow>(
            StoredProcedureNames.GetAnalyticsTrainerPerformance, new { GymId = gymId }, cancellationToken);

        return new TrainerAnalyticsDto
        {
            ActiveTrainers = summary?.ActiveTrainers ?? 0,
            AssignedMembers = summary?.AssignedMembers ?? 0,
            Performance = perf.Select(r => new TrainerPerformanceDto
            {
                TrainerId = r.TrainerId,
                TrainerName = r.TrainerName,
                AssignedMembers = r.AssignedMembers,
                TodayAttendance = r.TodayAttendance
            }).ToList()
        };
    }

    public async Task<WorkoutAnalyticsDto> GetWorkoutsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<WorkoutSummaryRow>(
            StoredProcedureNames.GetAnalyticsWorkoutSummary, new { GymId = gymId }, cancellationToken);
        return new WorkoutAnalyticsDto
        {
            ActiveWorkoutPlans = row?.ActiveWorkoutPlans ?? 0,
            CompletedWorkoutPlans = row?.CompletedWorkoutPlans ?? 0,
            CompletionPercentage = row?.CompletionPercentage ?? 0
        };
    }

    public async Task<DietAnalyticsDto> GetDietsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<DietSummaryRow>(
            StoredProcedureNames.GetAnalyticsDietSummary, new { GymId = gymId }, cancellationToken);
        return new DietAnalyticsDto
        {
            ActiveDietPlans = row?.ActiveDietPlans ?? 0,
            CompliancePercentage = row?.CompliancePercentage ?? 0
        };
    }

    public async Task<AnalyticsWidgetsDto> GetWidgetsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var payments = await _sp.QueryAsync<RecentPaymentRow>(
            StoredProcedureNames.GetAnalyticsRecentPayments, new { GymId = gymId, TopN = 5 }, cancellationToken);
        var expiring = await _sp.QueryAsync<ExpiringRow>(
            StoredProcedureNames.GetAnalyticsExpiringMemberships, new { GymId = gymId, TopN = 5 }, cancellationToken);
        var members = await _sp.QueryAsync<NewMemberRow>(
            StoredProcedureNames.GetAnalyticsNewMembers, new { GymId = gymId, TopN = 5 }, cancellationToken);
        var notifications = await _sp.QueryAsync<RecentNotifRow>(
            StoredProcedureNames.GetAnalyticsRecentNotifications, new { GymId = gymId, TopN = 5 }, cancellationToken);

        return new AnalyticsWidgetsDto
        {
            RecentPayments = payments.Select(r => new RecentPaymentWidgetDto
            {
                PaymentId = r.PaymentId, Amount = r.Amount, PaymentMethod = r.PaymentMethod,
                Status = r.Status, PaymentDate = r.PaymentDate, MemberName = r.MemberName
            }).ToList(),
            ExpiringMemberships = expiring.Select(r => new ExpiringMembershipWidgetDto
            {
                MembershipId = r.MembershipId, EndDate = DateOnly.FromDateTime(r.EndDate),
                MemberName = r.MemberName, PlanName = r.PlanName
            }).ToList(),
            NewMembers = members.Select(r => new NewMemberWidgetDto
            {
                MemberId = r.MemberId, MemberName = r.MemberName, MemberEmail = r.MemberEmail,
                JoinDate = DateOnly.FromDateTime(r.JoinDate)
            }).ToList(),
            RecentNotifications = notifications.Select(r => new RecentNotificationWidgetDto
            {
                LogId = r.LogId, NotificationType = r.NotificationType, Status = r.Status,
                RecipientPhone = r.RecipientPhone, CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    private static AnalyticsOverviewDto MapOverview(AnalyticsOverviewRow? row) => new()
    {
        TotalMembers = row?.TotalMembers ?? 0,
        ActiveMembers = row?.ActiveMembers ?? 0,
        RevenueToday = row?.RevenueToday ?? 0,
        RevenueThisMonth = row?.RevenueThisMonth ?? 0,
        ExpiringMemberships = row?.ExpiringMemberships ?? 0,
        ActiveTrainers = row?.ActiveTrainers ?? 0
    };

    private static MemberActivityDto MapActivity(MemberActivityRow r) => new()
    {
        MemberId = r.MemberId,
        MemberName = r.MemberName,
        AttendanceCount = r.AttendanceCount
    };
}

using Gym.Application.DTOs.Analytics;

namespace Gym.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<RevenueAnalyticsDto> GetRevenueAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<MembershipAnalyticsDto> GetMembershipAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<AttendanceAnalyticsDto> GetAttendanceAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<TrainerAnalyticsDto> GetTrainersAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<WorkoutAnalyticsDto> GetWorkoutsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<DietAnalyticsDto> GetDietsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<AnalyticsWidgetsDto> GetWidgetsAsync(Guid gymId, CancellationToken cancellationToken = default);
}

public interface IAnalyticsService
{
    Task<AnalyticsDashboardDto> GetDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<RevenueAnalyticsDto> GetRevenueAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<MembershipAnalyticsDto> GetMembershipAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<AttendanceAnalyticsDto> GetAttendanceAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<TrainerAnalyticsDto> GetTrainersAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<WorkoutAnalyticsDto> GetWorkoutsAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<DietAnalyticsDto> GetDietsAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<byte[]> ExportPdfAsync(string reportType, Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<byte[]> ExportExcelAsync(string reportType, Guid? gymId = null, CancellationToken cancellationToken = default);
}

public interface IAnalyticsReportExporter
{
    byte[] ExportDashboardPdf(AnalyticsDashboardDto dashboard, string gymName);
    byte[] ExportDashboardExcel(AnalyticsDashboardDto dashboard, string gymName);
    byte[] ExportRevenuePdf(RevenueAnalyticsDto revenue, string gymName);
    byte[] ExportRevenueExcel(RevenueAnalyticsDto revenue, string gymName);
}

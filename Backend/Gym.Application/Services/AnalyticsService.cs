using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Analytics;
using Gym.Application.DTOs.Audit;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repository;
    private readonly IAnalyticsReportExporter _exporter;
    private readonly IAuditService _auditService;
    private readonly IGymRepository _gymRepository;
    private readonly ICurrentUserService _currentUser;

    public AnalyticsService(
        IAnalyticsRepository repository,
        IAnalyticsReportExporter exporter,
        IAuditService auditService,
        IGymRepository gymRepository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _exporter = exporter;
        _auditService = auditService;
        _gymRepository = gymRepository;
        _currentUser = currentUser;
    }

    public async Task<AnalyticsDashboardDto> GetDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScope(gymId);
        var overview = await _repository.GetOverviewAsync(scope, cancellationToken);
        var revenue = await _repository.GetRevenueAsync(scope, cancellationToken);
        var membership = await _repository.GetMembershipAsync(scope, cancellationToken);
        var attendance = await _repository.GetAttendanceAsync(scope, cancellationToken);
        var trainers = await _repository.GetTrainersAsync(scope, cancellationToken);
        var workouts = await _repository.GetWorkoutsAsync(scope, cancellationToken);
        var diets = await _repository.GetDietsAsync(scope, cancellationToken);
        var widgets = await _repository.GetWidgetsAsync(scope, cancellationToken);

        return new AnalyticsDashboardDto
        {
            Overview = overview,
            Revenue = revenue,
            Membership = membership,
            Attendance = attendance,
            Trainers = trainers,
            Workouts = workouts,
            Diets = diets,
            Widgets = widgets
        };
    }

    public Task<RevenueAnalyticsDto> GetRevenueAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _repository.GetRevenueAsync(ResolveGymScope(gymId), cancellationToken);

    public Task<MembershipAnalyticsDto> GetMembershipAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _repository.GetMembershipAsync(ResolveGymScope(gymId), cancellationToken);

    public Task<AttendanceAnalyticsDto> GetAttendanceAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _repository.GetAttendanceAsync(ResolveGymScope(gymId), cancellationToken);

    public Task<TrainerAnalyticsDto> GetTrainersAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _repository.GetTrainersAsync(ResolveGymScope(gymId), cancellationToken);

    public Task<WorkoutAnalyticsDto> GetWorkoutsAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _repository.GetWorkoutsAsync(ResolveGymScope(gymId), cancellationToken);

    public Task<DietAnalyticsDto> GetDietsAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _repository.GetDietsAsync(ResolveGymScope(gymId), cancellationToken);

    public async Task<byte[]> ExportPdfAsync(string reportType, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScope(gymId);
        var gymName = await GetGymNameAsync(scope, cancellationToken);
        byte[] bytes;

        if (string.Equals(reportType, "revenue", StringComparison.OrdinalIgnoreCase))
        {
            var revenue = await _repository.GetRevenueAsync(scope, cancellationToken);
            bytes = _exporter.ExportRevenuePdf(revenue, gymName);
        }
        else
        {
            var dashboard = await GetDashboardAsync(gymId, cancellationToken);
            bytes = _exporter.ExportDashboardPdf(dashboard, gymName);
        }

        await LogExportAsync(scope, reportType, "pdf", cancellationToken);
        return bytes;
    }

    public async Task<byte[]> ExportExcelAsync(string reportType, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScope(gymId);
        var gymName = await GetGymNameAsync(scope, cancellationToken);
        byte[] bytes;

        if (string.Equals(reportType, "revenue", StringComparison.OrdinalIgnoreCase))
        {
            var revenue = await _repository.GetRevenueAsync(scope, cancellationToken);
            bytes = _exporter.ExportRevenueExcel(revenue, gymName);
        }
        else
        {
            var dashboard = await GetDashboardAsync(gymId, cancellationToken);
            bytes = _exporter.ExportDashboardExcel(dashboard, gymName);
        }

        await LogExportAsync(scope, reportType, "excel", cancellationToken);
        return bytes;
    }

    private Guid ResolveGymScope(Guid? requestedGymId) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);

    private async Task<string> GetGymNameAsync(Guid gymId, CancellationToken cancellationToken)
    {
        var gym = await _gymRepository.GetByIdAsync(gymId, cancellationToken);
        return gym?.Name ?? "Gym";
    }

    private Task LogExportAsync(Guid gymId, string reportType, string format, CancellationToken cancellationToken) =>
        _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Analytics,
            EntityId = reportType,
            ActionType = AuditActionTypes.Export,
            NewValue = new { reportType, format, exportedAt = DateTime.UtcNow }
        }, cancellationToken);
}

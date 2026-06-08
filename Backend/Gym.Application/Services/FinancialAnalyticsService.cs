using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class FinancialAnalyticsService : IFinancialAnalyticsService
{
    private readonly IFinancialAnalyticsRepository _analytics;
    private readonly IExpenseRepository _expenses;
    private readonly IPayrollRepository _payroll;
    private readonly IGymRepository _gymRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFinancialReportExporter _exporter;

    public FinancialAnalyticsService(
        IFinancialAnalyticsRepository analytics,
        IExpenseRepository expenses,
        IPayrollRepository payroll,
        IGymRepository gymRepository,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFinancialReportExporter exporter)
    {
        _analytics = analytics;
        _expenses = expenses;
        _payroll = payroll;
        _gymRepository = gymRepository;
        _currentUser = currentUser;
        _auditService = auditService;
        _exporter = exporter;
    }

    public async Task<FinancialDashboardDto> GetDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var scope = ResolveAnalyticsScope(gymId);
        var (revenue, commissions) = await _analytics.GetRevenueSummaryAsync(scope, cancellationToken);
        var expenseDash = await _expenses.GetDashboardAsync(scope, cancellationToken);
        var payrollDash = await _payroll.GetDashboardAsync(scope, cancellationToken);
        var summary = await _analytics.GetProfitLossAsync(scope, null, null, cancellationToken);
        var trend = await _analytics.GetMonthlyProfitTrendAsync(scope, 12, cancellationToken);
        var breakdown = await _expenses.GetCategoryBreakdownAsync(scope, null, null, cancellationToken);
        var payrollTrend = await _payroll.GetPayrollCostTrendAsync(scope, 12, cancellationToken);
        var commissionTrend = await _payroll.GetCommissionTrendAsync(scope, 12, cancellationToken);

        return new FinancialDashboardDto
        {
            RevenueThisMonth = revenue,
            ExpensesThisMonth = expenseDash.ExpensesThisMonth,
            ProfitThisMonth = revenue - expenseDash.ExpensesThisMonth - payrollDash.PayrollCostThisMonth,
            PendingSalaries = payrollDash.PendingSalaries,
            TotalTrainerCommissions = commissions,
            Summary = summary,
            ExpenseDashboard = expenseDash,
            PayrollDashboard = payrollDash,
            MonthlyProfitTrend = trend,
            ExpenseBreakdown = breakdown,
            PayrollCostTrend = payrollTrend,
            CommissionTrend = commissionTrend
        };
    }

    public async Task<ProfitLossSummaryDto> GetProfitLossAsync(
        Guid? gymId = null, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        return await _analytics.GetProfitLossAsync(ResolveAnalyticsScope(gymId), from, to, cancellationToken);
    }

    public async Task<byte[]> ExportProfitLossPdfAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(query.GymId, cancellationToken);
        var title = await GetTitleAsync(query.GymId, "Profit & Loss Report", cancellationToken);
        await LogExportAsync(query.GymId, "profit-loss", "pdf", cancellationToken);
        return _exporter.ExportProfitLossPdf(dashboard, title);
    }

    public async Task<byte[]> ExportProfitLossExcelAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(query.GymId, cancellationToken);
        var title = await GetTitleAsync(query.GymId, "Profit & Loss Report", cancellationToken);
        await LogExportAsync(query.GymId, "profit-loss", "excel", cancellationToken);
        return _exporter.ExportProfitLossExcel(dashboard, title);
    }

    private void EnsureCanView()
    {
        if (!_currentUser.HasPermission(Permissions.ViewFinancialAnalytics))
            throw new UnauthorizedAccessException();
    }

    private Guid? ResolveAnalyticsScope(Guid? requestedGymId)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
            return requestedGymId;
        return GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
    }

    private async Task<string> GetTitleAsync(Guid? gymId, string suffix, CancellationToken cancellationToken)
    {
        if (gymId is null && _currentUser.HasRole(RoleNames.SuperAdmin))
            return $"All Gyms — {suffix}";
        var scope = GymScopeResolver.ResolveRequired(_currentUser, gymId);
        var gym = await _gymRepository.GetByIdAsync(scope, cancellationToken);
        return $"{gym?.Name ?? "Gym"} — {suffix}";
    }

    private Task LogExportAsync(Guid? gymId, string reportType, string format, CancellationToken cancellationToken)
    {
        var scope = gymId ?? _currentUser.GymId;
        if (scope is null) return Task.CompletedTask;
        return _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = scope.Value,
            EntityName = AuditEntityNames.FinancialAnalytics,
            EntityId = reportType,
            ActionType = AuditActionTypes.Export,
            NewValue = new { reportType, format }
        }, cancellationToken);
    }
}

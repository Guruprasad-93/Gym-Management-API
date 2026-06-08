using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class FinancialAnalyticsRepository : IFinancialAnalyticsRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public FinancialAnalyticsRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<(decimal RevenueThisMonth, decimal CommissionsThisMonth)> GetRevenueSummaryAsync(
        Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<FinancialRevenueRow>(
            StoredProcedureNames.GetFinancialRevenueSummary, new { GymId = gymId }, cancellationToken);
        return (row?.RevenueThisMonth ?? 0, row?.CommissionsThisMonth ?? 0);
    }

    public async Task<ProfitLossSummaryDto> GetProfitLossAsync(
        Guid? gymId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<ProfitLossRow>(
            StoredProcedureNames.GetProfitLossSummary, new { GymId = gymId, FromDate = from, ToDate = to }, cancellationToken);
        return new ProfitLossSummaryDto
        {
            Revenue = row?.Revenue ?? 0,
            Expenses = row?.Expenses ?? 0,
            PayrollCost = row?.PayrollCost ?? 0,
            TrainerCommissions = row?.TrainerCommissions ?? 0,
            Profit = row?.Profit ?? 0,
            FromDate = row is null ? (from ?? DateOnly.FromDateTime(DateTime.UtcNow)) : DateOnly.FromDateTime(row.FromDate),
            ToDate = row is null ? (to ?? DateOnly.FromDateTime(DateTime.UtcNow)) : DateOnly.FromDateTime(row.ToDate)
        };
    }

    public async Task<IReadOnlyList<MonthlyProfitPointDto>> GetMonthlyProfitTrendAsync(
        Guid? gymId, int months, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MonthlyProfitRow>(
            StoredProcedureNames.GetMonthlyProfitTrend, new { GymId = gymId, Months = months }, cancellationToken);
        return rows.Select(r => new MonthlyProfitPointDto
        {
            Year = r.Year, Month = r.Month, MonthLabel = r.MonthLabel,
            Revenue = r.Revenue, Expenses = r.Expenses, PayrollCost = r.PayrollCost
        }).Reverse().ToList();
    }
}

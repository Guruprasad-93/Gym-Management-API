using Gym.Application.DTOs.Dashboard;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public DashboardRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<DashboardStatsDto> GetSuperAdminStatsAsync(CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<DashboardStatsRow>(
            StoredProcedureNames.GetDashboardStatistics,
            cancellationToken: cancellationToken);

        return Map(row);
    }

    public async Task<DashboardStatsDto> GetGymAdminStatsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<DashboardStatsRow>(
            StoredProcedureNames.GetGymDashboardStatistics,
            new { GymId = gymId },
            cancellationToken);

        return Map(row);
    }

    private static DashboardStatsDto Map(DashboardStatsRow? row) => new()
    {
        TotalGyms = row?.TotalGyms ?? 0,
        ActiveGyms = row?.ActiveGyms ?? 0,
        TotalMembers = row?.TotalMembers ?? 0,
        ActiveMembers = row?.ActiveMembers ?? 0,
        MembersWithTrainer = row?.MembersWithTrainer ?? 0,
        TotalRevenue = row?.TotalRevenue ?? 0,
        ExpiredMemberships = row?.ExpiredMemberships ?? 0,
        ActiveMemberships = row?.ActiveMemberships ?? 0,
        PendingRenewals = row?.PendingRenewals ?? 0,
        MonthlyRevenue = row?.MonthlyRevenue ?? 0,
        TotalTrainers = row?.TotalTrainers ?? 0
    };
}

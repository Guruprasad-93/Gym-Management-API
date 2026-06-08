using Gym.Application.DTOs.Dashboard;

namespace Gym.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardStatsDto> GetSuperAdminStatsAsync(CancellationToken cancellationToken = default);
    Task<DashboardStatsDto> GetGymAdminStatsAsync(Guid gymId, CancellationToken cancellationToken = default);
}

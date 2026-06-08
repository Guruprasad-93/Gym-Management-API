using Gym.Application.DTOs.Dashboard;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IDashboardRepository dashboardRepository, ICurrentUserService currentUser)
    {
        _dashboardRepository = dashboardRepository;
        _currentUser = currentUser;
    }

    public Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.HasRole("SuperAdmin") && _currentUser.GymId is null)
            return _dashboardRepository.GetSuperAdminStatsAsync(cancellationToken);

        return _dashboardRepository.GetGymAdminStatsAsync(_currentUser.RequireGymId(), cancellationToken);
    }
}

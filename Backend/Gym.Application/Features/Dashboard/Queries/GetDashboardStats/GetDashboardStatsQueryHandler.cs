using Gym.Application.DTOs.Dashboard;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IDashboardService _dashboardService;

    public GetDashboardStatsQueryHandler(IDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    public Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken) =>
        _dashboardService.GetStatsAsync(cancellationToken);
}

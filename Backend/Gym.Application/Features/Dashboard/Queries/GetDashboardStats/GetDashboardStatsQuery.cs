using Gym.Application.DTOs.Dashboard;
using MediatR;

namespace Gym.Application.Features.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

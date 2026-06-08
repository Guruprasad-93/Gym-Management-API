using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Dashboard;
using Gym.Application.Features.Dashboard.Queries.GetDashboardStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("stats")]
    [RequirePermission(Permissions.ViewDashboard)]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _mediator.Send(new GetDashboardStatsQuery(), cancellationToken);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }
}

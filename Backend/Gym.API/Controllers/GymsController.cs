using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Gyms;
using Gym.Application.Features.Gyms.Commands.CreateGym;
using Gym.Application.Features.Gyms.Queries.GetAllGyms;
using Gym.Application.Features.Gyms.Queries.GetGymById;
using Gym.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GymsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGymService _gymService;

    public GymsController(IMediator mediator, IGymService gymService)
    {
        _mediator = mediator;
        _gymService = gymService;
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewGyms)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GymDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var gyms = await _mediator.Send(new GetAllGymsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GymDto>>.Ok(gyms));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.ViewGyms)]
    public async Task<ActionResult<ApiResponse<GymDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var gym = await _mediator.Send(new GetGymByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<GymDto>.Ok(gym));
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateGym)]
    public async Task<ActionResult<ApiResponse<GymDto>>> Create([FromBody] CreateGymDto dto, CancellationToken cancellationToken)
    {
        var gym = await _mediator.Send(new CreateGymCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<GymDto>.Ok(gym, "Gym created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.UpdateGym)]
    public async Task<ActionResult<ApiResponse<GymDto>>> Update(Guid id, [FromBody] UpdateGymDto dto, CancellationToken cancellationToken)
    {
        var gym = await _gymService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<GymDto>.Ok(gym, "Gym updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.DeleteGym)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _gymService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Gym deleted successfully."));
    }

    [HttpPatch("{id:guid}/activate")]
    [RequirePermission(Permissions.ActivateGym)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _gymService.SetActiveAsync(id, true, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Gym activated."));
    }

    [HttpPatch("{id:guid}/deactivate")]
    [RequirePermission(Permissions.DeactivateGym)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _gymService.SetActiveAsync(id, false, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Gym deactivated."));
    }
}

using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Trainers;
using Gym.Application.Features.Trainers.Commands.AssignMembersToTrainer;
using Gym.Application.Features.Trainers.Commands.CreateTrainer;
using Gym.Application.Features.Trainers.Commands.DeleteTrainer;
using Gym.Application.Features.Trainers.Commands.RemoveMemberAssignment;
using Gym.Application.Features.Trainers.Commands.UpdateTrainer;
using Gym.Application.Features.Trainers.Queries.GetCurrentTrainer;
using Gym.Application.Features.Trainers.Queries.GetTrainerById;
using Gym.Application.Features.Trainers.Queries.GetTrainerDashboard;
using Gym.Application.Features.Trainers.Queries.GetTrainerMembers;
using Gym.Application.Features.Trainers.Queries.GetTrainers;
using Gym.Application.Features.Trainers.Queries.GetUnassignedMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/trainers")]
[Authorize]
public class TrainersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(Permissions.ViewTrainers)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<TrainerDto>>>> GetAll(
        [FromQuery] Guid? gymId,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive,
        [FromQuery] PagedRequestDto paging,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTrainersQuery(gymId, search, includeInactive, paging),
            cancellationToken);
        return Ok(ApiResponse<PagedResultDto<TrainerDto>>.Ok(result));
    }

    [HttpGet("me")]
    [RequirePermission(Permissions.ViewTrainers)]
    public async Task<ActionResult<ApiResponse<TrainerDto>>> GetMe(CancellationToken cancellationToken)
    {
        var trainer = await _mediator.Send(new GetCurrentTrainerQuery(), cancellationToken);
        return Ok(ApiResponse<TrainerDto>.Ok(trainer));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewTrainers)]
    public async Task<ActionResult<ApiResponse<TrainerDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var trainer = await _mediator.Send(new GetTrainerByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<TrainerDto>.Ok(trainer));
    }

    [HttpGet("{id:int}/dashboard")]
    [RequirePermission(Permissions.ViewTrainers)]
    public async Task<ActionResult<ApiResponse<TrainerDashboardDto>>> GetDashboard(
        int id,
        CancellationToken cancellationToken)
    {
        var dashboard = await _mediator.Send(new GetTrainerDashboardQuery(id), cancellationToken);
        return Ok(ApiResponse<TrainerDashboardDto>.Ok(dashboard));
    }

    [HttpGet("{id:int}/members")]
    [RequirePermission(Permissions.ViewTrainers)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberDto>>>> GetMembers(
        int id,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var members = await _mediator.Send(new GetTrainerMembersQuery(id, search), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberDto>>.Ok(members));
    }

    [HttpGet("{id:int}/unassigned-members")]
    [RequirePermission(Permissions.AssignMemberToTrainer)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberDto>>>> GetUnassignedMembers(
        int id,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var members = await _mediator.Send(new GetUnassignedMembersQuery(id, search), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MemberDto>>.Ok(members));
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateTrainer)]
    public async Task<ActionResult<ApiResponse<TrainerDto>>> Create(
        [FromBody] CreateTrainerDto dto,
        CancellationToken cancellationToken)
    {
        var trainer = await _mediator.Send(new CreateTrainerCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TrainerDto>.Ok(trainer, "Trainer created successfully."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.UpdateTrainer)]
    public async Task<ActionResult<ApiResponse<TrainerDto>>> Update(
        int id,
        [FromBody] UpdateTrainerDto dto,
        CancellationToken cancellationToken)
    {
        var trainer = await _mediator.Send(new UpdateTrainerCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<TrainerDto>.Ok(trainer, "Trainer updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.DeleteTrainer)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTrainerCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Trainer deleted successfully."));
    }

    [HttpPost("{id:int}/assign-members")]
    [RequirePermission(Permissions.AssignMemberToTrainer)]
    public async Task<ActionResult<ApiResponse<object>>> AssignMembers(
        int id,
        [FromBody] AssignMembersToTrainerDto dto,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AssignMembersToTrainerCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Members assigned successfully."));
    }

    [HttpDelete("members/{memberId:int}/assignment")]
    [RequirePermission(Permissions.AssignMemberToTrainer)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveMemberAssignment(
        int memberId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveMemberAssignmentCommand(memberId), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Member assignment removed."));
    }
}

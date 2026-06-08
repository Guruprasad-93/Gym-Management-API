using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.Features.Members.Commands.ActivateMember;
using Gym.Application.Features.Members.Commands.AssignTrainer;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Application.Features.Members.Commands.DeactivateMember;
using Gym.Application.Features.Members.Commands.DeleteMember;
using Gym.Application.Features.Members.Commands.RemoveTrainerAssignment;
using Gym.Application.Features.Members.Commands.UpdateMember;
using Gym.Application.Features.Members.Queries.GetCurrentMember;
using Gym.Application.Features.Members.Queries.GetMemberById;
using Gym.Application.Features.Members.Queries.GetMemberDetails;
using Gym.Application.Features.Members.Queries.GetMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/members")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(Permissions.ViewMembers)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<MemberResponseDto>>>> GetAll(
        [FromQuery] Guid? gymId,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive,
        [FromQuery] PagedRequestDto paging,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetMembersQuery(gymId, search, includeInactive, paging),
            cancellationToken);
        return Ok(ApiResponse<PagedResultDto<MemberResponseDto>>.Ok(result));
    }

    [HttpGet("me")]
    [RequirePermission(Permissions.ViewMemberDetails)]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> GetMe(CancellationToken cancellationToken)
    {
        var member = await _mediator.Send(new GetCurrentMemberQuery(), cancellationToken);
        return Ok(ApiResponse<MemberResponseDto>.Ok(member));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewMembers)]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var member = await _mediator.Send(new GetMemberByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<MemberResponseDto>.Ok(member));
    }

    [HttpGet("{id:int}/details")]
    [RequireAnyPermission(Permissions.ViewMemberDetails, Permissions.ViewMembers)]
    public async Task<ActionResult<ApiResponse<MemberDetailsDto>>> GetDetails(int id, CancellationToken cancellationToken)
    {
        var details = await _mediator.Send(new GetMemberDetailsQuery(id), cancellationToken);
        return Ok(ApiResponse<MemberDetailsDto>.Ok(details));
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateMember)]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> Create(
        [FromBody] CreateMemberDto dto,
        CancellationToken cancellationToken)
    {
        var member = await _mediator.Send(new CreateMemberCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MemberResponseDto>.Ok(member, "Member created successfully."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.UpdateMember)]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> Update(
        int id,
        [FromBody] UpdateMemberDto dto,
        CancellationToken cancellationToken)
    {
        var member = await _mediator.Send(new UpdateMemberCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<MemberResponseDto>.Ok(member, "Member updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.DeleteMember)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMemberCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Member deleted successfully."));
    }

    [HttpPatch("{id:int}/activate")]
    [RequirePermission(Permissions.UpdateMember)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateMemberCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Member activated."));
    }

    [HttpPatch("{id:int}/deactivate")]
    [RequirePermission(Permissions.UpdateMember)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateMemberCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Member deactivated."));
    }

    [HttpPost("{id:int}/assign-trainer")]
    [RequirePermission(Permissions.AssignTrainer)]
    public async Task<ActionResult<ApiResponse<object>>> AssignTrainer(
        int id,
        [FromBody] AssignTrainerToMemberDto dto,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AssignTrainerCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Trainer assigned successfully."));
    }

    [HttpDelete("{id:int}/trainer-assignment")]
    [RequirePermission(Permissions.AssignTrainer)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveTrainerAssignment(
        int id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveTrainerAssignmentCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Trainer assignment removed."));
    }
}

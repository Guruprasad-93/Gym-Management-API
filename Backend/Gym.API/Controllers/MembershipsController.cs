using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Memberships;
using Gym.Application.Features.Memberships.Commands.CancelMembership;
using Gym.Application.Features.Memberships.Commands.CreateMembership;
using Gym.Application.Features.Memberships.Commands.RenewMembership;
using Gym.Application.Features.Memberships.Queries.GetExpiredMemberships;
using Gym.Application.Features.Memberships.Queries.GetMembershipById;
using Gym.Application.Features.Memberships.Queries.GetMemberships;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/memberships")]
[Authorize]
public class MembershipsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembershipsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(Permissions.ViewMemberships)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MembershipResponseDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var list = await _mediator.Send(new GetMembershipsQuery(search, includeInactive), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MembershipResponseDto>>.Ok(list));
    }

    [HttpGet("expired")]
    [RequirePermission(Permissions.ViewMemberships)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MembershipResponseDto>>>> GetExpired(
        CancellationToken cancellationToken)
    {
        var list = await _mediator.Send(new GetExpiredMembershipsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MembershipResponseDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ViewMemberships)]
    public async Task<ActionResult<ApiResponse<MembershipResponseDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var membership = await _mediator.Send(new GetMembershipByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<MembershipResponseDto>.Ok(membership));
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateMembership)]
    public async Task<ActionResult<ApiResponse<MembershipResponseDto>>> Create(
        [FromBody] CreateMembershipDto dto,
        CancellationToken cancellationToken)
    {
        var membership = await _mediator.Send(new CreateMembershipCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MembershipResponseDto>.Ok(membership, "Membership created."));
    }

    [HttpPost("{id:int}/renew")]
    [RequirePermission(Permissions.RenewMembership)]
    public async Task<ActionResult<ApiResponse<MembershipResponseDto>>> Renew(
        int id,
        [FromBody] RenewMembershipDto dto,
        CancellationToken cancellationToken)
    {
        var membership = await _mediator.Send(new RenewMembershipCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<MembershipResponseDto>.Ok(membership, "Membership renewed."));
    }

    [HttpPost("{id:int}/cancel")]
    [RequirePermission(Permissions.UpdateMembership)]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(
        int id,
        [FromBody] CancelMembershipDto dto,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelMembershipCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Membership cancelled."));
    }
}

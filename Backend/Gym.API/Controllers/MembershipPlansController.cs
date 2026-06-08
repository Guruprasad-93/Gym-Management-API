using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Memberships;
using Gym.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using Gym.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;
using Gym.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;
using Gym.Application.Features.MembershipPlans.Queries.GetMembershipPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/membership-plans")]
[Authorize]
public class MembershipPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembershipPlansController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(Permissions.ViewMemberships)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MembershipPlanResponseDto>>>> GetAll(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var plans = await _mediator.Send(new GetMembershipPlansQuery(includeInactive), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MembershipPlanResponseDto>>.Ok(plans));
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateMembership)]
    public async Task<ActionResult<ApiResponse<MembershipPlanResponseDto>>> Create(
        [FromBody] CreateMembershipPlanDto dto,
        CancellationToken cancellationToken)
    {
        var plan = await _mediator.Send(new CreateMembershipPlanCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MembershipPlanResponseDto>.Ok(plan, "Plan created."));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.UpdateMembership)]
    public async Task<ActionResult<ApiResponse<MembershipPlanResponseDto>>> Update(
        int id,
        [FromBody] UpdateMembershipPlanDto dto,
        CancellationToken cancellationToken)
    {
        var plan = await _mediator.Send(new UpdateMembershipPlanCommand(id, dto), cancellationToken);
        return Ok(ApiResponse<MembershipPlanResponseDto>.Ok(plan, "Plan updated."));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.UpdateMembership)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMembershipPlanCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Plan deleted."));
    }
}

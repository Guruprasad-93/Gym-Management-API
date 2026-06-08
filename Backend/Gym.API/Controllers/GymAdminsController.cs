using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Features.GymAdmins.Commands.CreateGymAdmin;
using Gym.Application.Features.GymAdmins.Commands.ResendTemporaryPassword;
using Gym.Application.Features.GymAdmins.Commands.SetGymAdminActive;
using Gym.Application.Features.GymAdmins.Commands.UpdateGymAdmin;
using Gym.Application.Features.GymAdmins.Queries.GetGymAdminById;
using Gym.Application.Features.GymAdmins.Queries.GetGymAdmins;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/gym-admins")]
[Authorize]
public class GymAdminsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GymAdminsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(Permissions.ViewGymAdmins)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<GymAdminDto>>>> GetAll(
        [FromQuery] Guid? gymId,
        [FromQuery] PagedRequestDto paging,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGymAdminsQuery(gymId, paging), cancellationToken);
        return Ok(ApiResponse<PagedResultDto<GymAdminDto>>.Ok(result));
    }

    [HttpGet("{userId:guid}")]
    [RequirePermission(Permissions.ViewGymAdmins)]
    public async Task<ActionResult<ApiResponse<GymAdminDto>>> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var admin = await _mediator.Send(new GetGymAdminByIdQuery(userId), cancellationToken);
        return Ok(ApiResponse<GymAdminDto>.Ok(admin));
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateGymAdmin)]
    public async Task<ActionResult<ApiResponse<CreateGymAdminResultDto>>> Create(
        [FromBody] CreateGymAdminDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateGymAdminCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateGymAdminResultDto>.Ok(result, result.Message));
    }

    [HttpPut("{userId:guid}")]
    [RequirePermission(Permissions.UpdateGymAdmin)]
    public async Task<ActionResult<ApiResponse<GymAdminDto>>> Update(
        Guid userId,
        [FromBody] UpdateGymAdminDto dto,
        CancellationToken cancellationToken)
    {
        var admin = await _mediator.Send(new UpdateGymAdminCommand(userId, dto), cancellationToken);
        return Ok(ApiResponse<GymAdminDto>.Ok(admin, "Gym admin updated successfully."));
    }

    [HttpPatch("{userId:guid}/activate")]
    [RequirePermission(Permissions.UpdateGymAdmin)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetGymAdminActiveCommand(userId, true), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Gym admin activated."));
    }

    [HttpPatch("{userId:guid}/deactivate")]
    [RequirePermission(Permissions.DeleteGymAdmin)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetGymAdminActiveCommand(userId, false), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Gym admin deactivated."));
    }

    [HttpPost("{userId:guid}/resend-temporary-password")]
    [RequirePermission(Permissions.ResetGymAdminPassword)]
    public async Task<ActionResult<ApiResponse<ResendTemporaryPasswordResultDto>>> ResendTemporaryPassword(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResendTemporaryPasswordCommand(userId), cancellationToken);
        return Ok(ApiResponse<ResendTemporaryPasswordResultDto>.Ok(result, result.Message));
    }
}

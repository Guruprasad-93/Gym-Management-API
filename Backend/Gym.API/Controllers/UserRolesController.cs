using Gym.Application.Authorization;
using Gym.Application.DTOs.Authorization;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/user-roles")]
[Authorize]
public class UserRolesController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    public UserRolesController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    [HttpGet("user/{userId:guid}")]
    [RequirePermission("VIEW_USER_ROLES")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserRoleDto>>>> GetByUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _userRoleService.GetByUserIdAsync(userId, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<UserRoleDto>>.Ok(roles));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IReadOnlyList<UserRoleDto>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    [RequirePermission("ASSIGN_USER_ROLE")]
    public async Task<ActionResult<ApiResponse<object>>> Assign(
        [FromBody] AssignUserRoleDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userRoleService.AssignAsync(dto, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Role assigned to user successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("user/{userId:guid}/role/{roleId:int}")]
    [RequirePermission("REMOVE_USER_ROLE")]
    public async Task<ActionResult<ApiResponse<object>>> Remove(
        Guid userId,
        int roleId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userRoleService.RemoveAsync(userId, roleId, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Role removed from user successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

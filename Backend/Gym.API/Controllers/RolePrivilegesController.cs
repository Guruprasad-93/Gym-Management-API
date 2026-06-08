using Gym.Application.Authorization;
using Gym.Application.DTOs.Authorization;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/role-privileges")]
[Authorize]
public class RolePrivilegesController : ControllerBase
{
    private readonly IRolePrivilegeService _rolePrivilegeService;

    public RolePrivilegesController(IRolePrivilegeService rolePrivilegeService)
    {
        _rolePrivilegeService = rolePrivilegeService;
    }

    [HttpGet("matrix")]
    [RequirePermission("VIEW_PERMISSION_MATRIX")]
    public async Task<ActionResult<ApiResponse<RolePermissionMatrixDto>>> GetMatrix(CancellationToken cancellationToken)
    {
        var matrix = await _rolePrivilegeService.GetMatrixAsync(cancellationToken);
        return Ok(ApiResponse<RolePermissionMatrixDto>.Ok(matrix));
    }

    [HttpGet("role/{roleId:int}")]
    [RequirePermission("VIEW_ROLE_PRIVILEGES")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RolePrivilegeDto>>>> GetByRole(
        int roleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var privileges = await _rolePrivilegeService.GetByRoleIdAsync(roleId, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<RolePrivilegeDto>>.Ok(privileges));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IReadOnlyList<RolePrivilegeDto>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    [RequirePermission("ASSIGN_ROLE_PRIVILEGE")]
    public async Task<ActionResult<ApiResponse<object>>> Assign(
        [FromBody] AssignRolePrivilegeDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _rolePrivilegeService.AssignAsync(dto, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Privilege assigned to role successfully."));
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

    [HttpDelete("role/{roleId:int}/privilege/{privilegeId:int}")]
    [RequirePermission("REMOVE_ROLE_PRIVILEGE")]
    public async Task<ActionResult<ApiResponse<object>>> Remove(
        int roleId,
        int privilegeId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _rolePrivilegeService.RemoveAsync(roleId, privilegeId, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Privilege removed from role successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

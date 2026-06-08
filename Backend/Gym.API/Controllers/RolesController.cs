using Gym.Application.Authorization;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Roles;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("{id:int}")]
    [RequirePermission("VIEW_ROLES")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<RoleDto>.Ok(role));
    }

    [HttpGet]
    [RequirePermission("VIEW_ROLES")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoleDto>>.Ok(roles));
    }

    [HttpPost]
    [RequirePermission("CREATE_ROLE")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(
        [FromBody] CreateRoleDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.CreateAsync(dto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<RoleDto>.Ok(role, "Role created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<RoleDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<RoleDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [RequirePermission("UPDATE_ROLE")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(
        int id,
        [FromBody] UpdateRoleDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<RoleDto>.Ok(role, "Role updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<RoleDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<RoleDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [RequirePermission("DELETE_ROLE")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.DeleteAsync(id, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Role deleted successfully."));
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
}

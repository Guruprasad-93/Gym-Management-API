using Gym.Application.Authorization;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Privileges;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrivilegesController : ControllerBase
{
    private readonly IPrivilegeService _privilegeService;

    public PrivilegesController(IPrivilegeService privilegeService)
    {
        _privilegeService = privilegeService;
    }

    [HttpGet("{id:int}")]
    [RequirePermission("VIEW_PRIVILEGES")]
    public async Task<ActionResult<ApiResponse<PrivilegeDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var privilege = await _privilegeService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PrivilegeDto>.Ok(privilege));
    }

    [HttpGet]
    [RequirePermission("VIEW_PRIVILEGES")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrivilegeDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var privileges = await _privilegeService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PrivilegeDto>>.Ok(privileges));
    }

    [HttpPost]
    [RequirePermission("CREATE_PRIVILEGE")]
    public async Task<ActionResult<ApiResponse<PrivilegeDto>>> Create(
        [FromBody] CreatePrivilegeDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var privilege = await _privilegeService.CreateAsync(dto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<PrivilegeDto>.Ok(privilege, "Privilege created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PrivilegeDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PrivilegeDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [RequirePermission("UPDATE_PRIVILEGE")]
    public async Task<ActionResult<ApiResponse<PrivilegeDto>>> Update(
        int id,
        [FromBody] UpdatePrivilegeDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var privilege = await _privilegeService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<PrivilegeDto>.Ok(privilege, "Privilege updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PrivilegeDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PrivilegeDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PrivilegeDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [RequirePermission("DELETE_PRIVILEGE")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _privilegeService.DeleteAsync(id, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null!, "Privilege deleted successfully."));
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

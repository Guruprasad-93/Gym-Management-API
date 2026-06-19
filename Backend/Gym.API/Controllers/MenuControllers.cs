using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Menus;
using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/menus")]
[Authorize]
public class MenusController : ControllerBase
{
    private readonly IGymMenuService _menuService;

    public MenusController(IGymMenuService menuService) => _menuService = menuService;

    [HttpGet("my-menus")]
    [ProducesResponseType(typeof(ApiResponse<MyMenusResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MyMenusResponseDto>>> GetMyMenus(CancellationToken cancellationToken)
    {
        var menus = await _menuService.GetMyMenusAsync(cancellationToken);
        return Ok(ApiResponse<MyMenusResponseDto>.Ok(menus));
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewTenantMenus)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MenuDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuDto>>>> GetAllMenus(CancellationToken cancellationToken)
    {
        var menus = await _menuService.GetAllMenusAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MenuDto>>.Ok(menus));
    }
}

[ApiController]
[Route("api/platform/tenant-menus")]
[Authorize]
public class TenantMenuController : ControllerBase
{
    private readonly IGymMenuService _menuService;

    public TenantMenuController(IGymMenuService menuService) => _menuService = menuService;

    [HttpGet("gyms")]
    [RequirePermission(Permissions.ViewTenantMenus)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GymMenuSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GymMenuSummaryDto>>>> GetGymSummaries(CancellationToken cancellationToken)
    {
        var summaries = await _menuService.GetGymSummariesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GymMenuSummaryDto>>.Ok(summaries));
    }

    [HttpGet("{gymId:guid}")]
    [RequirePermission(Permissions.ViewTenantMenus)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TenantMenuDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantMenuDto>>>> GetGymMenus(
        Guid gymId,
        CancellationToken cancellationToken)
    {
        var menus = await _menuService.GetGymMenusAsync(gymId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TenantMenuDto>>.Ok(menus));
    }

    [HttpPut("{gymId:guid}/{menuId:int}/enable")]
    [RequirePermission(Permissions.ManageTenantMenus)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> EnableMenu(
        Guid gymId,
        int menuId,
        CancellationToken cancellationToken)
    {
        await _menuService.SetMenuEnabledAsync(gymId, menuId, true, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Menu enabled successfully."));
    }

    [HttpPut("{gymId:guid}/{menuId:int}/disable")]
    [RequirePermission(Permissions.ManageTenantMenus)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DisableMenu(
        Guid gymId,
        int menuId,
        CancellationToken cancellationToken)
    {
        await _menuService.SetMenuEnabledAsync(gymId, menuId, false, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Menu disabled successfully."));
    }

    [HttpPut("{gymId:guid}/bulk")]
    [RequirePermission(Permissions.ManageTenantMenus)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> BulkSetMenus(
        Guid gymId,
        [FromBody] BulkSetGymMenusDto dto,
        CancellationToken cancellationToken)
    {
        await _menuService.BulkSetMenusEnabledAsync(gymId, dto, cancellationToken);
        var action = dto.IsEnabled ? "enabled" : "disabled";
        return Ok(ApiResponse<object>.Ok(null!, $"Menus {action} successfully."));
    }
}

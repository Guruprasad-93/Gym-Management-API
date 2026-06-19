using Gym.Application.DTOs.Menus;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class GymMenuRepository : IGymMenuRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public GymMenuRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<IReadOnlyList<MenuDto>> GetAllMenusAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MenuRow>(StoredProcedureNames.MenuGetAll, cancellationToken: cancellationToken);
        return rows.Select(ToMenuDto).ToList();
    }

    public async Task<IReadOnlyList<string>> GetEnabledMenuCodesAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MenuCodeRow>(
            StoredProcedureNames.GymMenuGetEnabledCodes,
            new { GymId = gymId },
            cancellationToken);

        return rows.Select(r => r.MenuCode).ToList();
    }

    public async Task<bool> IsMenuEnabledAsync(Guid gymId, string menuCode, CancellationToken cancellationToken = default)
    {
        var result = await _sp.QuerySingleOrDefaultAsync<IsEnabledRow>(
            StoredProcedureNames.GymMenuIsEnabled,
            new { GymId = gymId, MenuCode = menuCode },
            cancellationToken);

        return result?.IsEnabled ?? false;
    }

    public async Task<IReadOnlyList<TenantMenuDto>> GetGymMenusAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymMenuRow>(
            StoredProcedureNames.GymMenuGetByGymId,
            new { GymId = gymId },
            cancellationToken);

        return rows.Select(ToTenantMenuDto).ToList();
    }

    public Task SetMenuEnabledAsync(
        Guid gymId,
        int menuId,
        bool isEnabled,
        Guid? enabledBy,
        CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.GymMenuSetEnabled,
            new { GymId = gymId, MenuId = menuId, IsEnabled = isEnabled, EnabledBy = enabledBy },
            cancellationToken);

    public Task BulkSetMenusEnabledAsync(
        Guid gymId,
        IReadOnlyList<int> menuIds,
        bool isEnabled,
        Guid? enabledBy,
        CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.GymMenuBulkSetEnabled,
            new
            {
                GymId = gymId,
                MenuIds = string.Join(',', menuIds),
                IsEnabled = isEnabled,
                EnabledBy = enabledBy
            },
            cancellationToken);

    public Task SeedMenusForGymAsync(Guid gymId, Guid? enabledBy, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.GymMenuSeedForGym,
            new { GymId = gymId, EnabledBy = enabledBy },
            cancellationToken);

    private static MenuDto ToMenuDto(MenuRow row) => new()
    {
        MenuId = row.MenuId,
        MenuCode = row.MenuCode,
        MenuName = row.MenuName,
        ParentMenuId = row.ParentMenuId,
        Route = row.Route,
        Icon = row.Icon,
        SortOrder = row.SortOrder,
        IsEnabled = true
    };

    private static TenantMenuDto ToTenantMenuDto(GymMenuRow row) => new()
    {
        GymMenuId = row.GymMenuId,
        GymId = row.GymId,
        MenuId = row.MenuId,
        MenuCode = row.MenuCode,
        MenuName = row.MenuName,
        ParentMenuId = row.ParentMenuId,
        Route = row.Route,
        Icon = row.Icon,
        SortOrder = row.SortOrder,
        IsEnabled = row.IsEnabled,
        EnabledOn = row.EnabledOn,
        EnabledBy = row.EnabledBy
    };

    private sealed class MenuCodeRow
    {
        public string MenuCode { get; set; } = string.Empty;
    }

    private sealed class IsEnabledRow
    {
        public bool IsEnabled { get; set; }
    }
}

using Gym.Application.Authorization;
using Gym.Application.DTOs.Menus;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class GymMenuService : IGymMenuService
{
    private readonly IGymMenuRepository _menuRepository;
    private readonly IGymRepository _gymRepository;
    private readonly IFeatureResolverService _featureResolver;
    private readonly ICurrentUserService _currentUser;

    public GymMenuService(
        IGymMenuRepository menuRepository,
        IGymRepository gymRepository,
        IFeatureResolverService featureResolver,
        ICurrentUserService currentUser)
    {
        _menuRepository = menuRepository;
        _gymRepository = gymRepository;
        _featureResolver = featureResolver;
        _currentUser = currentUser;
    }

    public async Task<MyMenusResponseDto> GetMyMenusAsync(CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.GymId;
        if (!gymId.HasValue || _currentUser.HasRole(RoleNames.SuperAdmin))
        {
            var allMenus = await GetAllMenusAsync(cancellationToken);
            return new MyMenusResponseDto
            {
                Menus = allMenus,
                EnabledMenuCodes = allMenus.Select(m => m.MenuCode).ToList()
            };
        }

        var visibleCodes = await _featureResolver.GetVisibleMenuCodesAsync(gymId.Value, cancellationToken);
        var visibleSet = visibleCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var permissions = _currentUser.Permissions;
        var accessibleCodes = await _featureResolver.GetAccessibleMenuCodesAsync(gymId.Value, permissions, cancellationToken);

        var gymMenus = await _menuRepository.GetGymMenusAsync(gymId.Value, cancellationToken);
        var visible = gymMenus
            .Where(m => m.IsEnabled && visibleSet.Contains(m.MenuCode))
            .Where(m => MenuPermissionMap.UserCanSeeMenu(m.MenuCode, permissions))
            .Select(m => new MenuDto
            {
                MenuId = m.MenuId,
                MenuCode = m.MenuCode,
                MenuName = m.MenuName,
                ParentMenuId = m.ParentMenuId,
                Route = m.Route,
                Icon = m.Icon,
                SortOrder = m.SortOrder,
                IsEnabled = m.IsEnabled,
                EnabledOn = m.EnabledOn,
                EnabledBy = m.EnabledBy
            })
            .ToList();

        visible = IncludeVisibleParents(visible, gymMenus.Where(m => m.IsEnabled && visibleSet.Contains(m.MenuCode)).ToList());

        return new MyMenusResponseDto
        {
            Menus = visible.OrderBy(m => m.SortOrder).ThenBy(m => m.MenuName).ToList(),
            EnabledMenuCodes = accessibleCodes
        };
    }

    public Task<IReadOnlyList<MenuDto>> GetAllMenusAsync(CancellationToken cancellationToken = default) =>
        _menuRepository.GetAllMenusAsync(cancellationToken);

    public Task<IReadOnlyList<TenantMenuDto>> GetGymMenusAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _menuRepository.GetGymMenusAsync(gymId, cancellationToken);

    public async Task<IReadOnlyList<GymMenuSummaryDto>> GetGymSummariesAsync(CancellationToken cancellationToken = default)
    {
        var gyms = await _gymRepository.GetAllAsync(cancellationToken);
        var allMenus = await _menuRepository.GetAllMenusAsync(cancellationToken);
        var totalMenus = allMenus.Count;

        var summaries = new List<GymMenuSummaryDto>();
        foreach (var gym in gyms)
        {
            var visibleCodes = await _featureResolver.GetVisibleMenuCodesAsync(gym.Id, cancellationToken);
            summaries.Add(new GymMenuSummaryDto
            {
                GymId = gym.Id,
                GymName = gym.Name,
                TotalMenus = totalMenus,
                EnabledMenus = visibleCodes.Count
            });
        }

        return summaries;
    }

    public async Task SetMenuEnabledAsync(Guid gymId, int menuId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        await _menuRepository.SetMenuEnabledAsync(gymId, menuId, isEnabled, _currentUser.UserId, cancellationToken);
        InvalidateCache(gymId);
    }

    public async Task BulkSetMenusEnabledAsync(Guid gymId, BulkSetGymMenusDto dto, CancellationToken cancellationToken = default)
    {
        EnsureSuperAdmin();
        if (dto.MenuIds.Count == 0)
            return;

        await _menuRepository.BulkSetMenusEnabledAsync(gymId, dto.MenuIds, dto.IsEnabled, _currentUser.UserId, cancellationToken);
        InvalidateCache(gymId);
    }

    public Task<bool> IsMenuEnabledAsync(Guid gymId, string menuCode, CancellationToken cancellationToken = default) =>
        _featureResolver.IsMenuVisibleAsync(gymId, menuCode, cancellationToken);

    public Task SeedMenusForGymAsync(Guid gymId, Guid? enabledBy, CancellationToken cancellationToken = default) =>
        _menuRepository.SeedMenusForGymAsync(gymId, enabledBy, cancellationToken);

    public void InvalidateCache(Guid gymId)
    {
    }

    private void EnsureSuperAdmin()
    {
        if (!_currentUser.HasRole(RoleNames.SuperAdmin))
            throw new UnauthorizedAccessException("Only Super Admin can manage tenant menus.");
    }

    private static List<MenuDto> IncludeVisibleParents(List<MenuDto> visible, IReadOnlyList<TenantMenuDto> allEnabled)
    {
        var visibleIds = visible.Select(m => m.MenuId).ToHashSet();
        var result = visible.ToList();

        foreach (var menu in visible)
        {
            var parentId = menu.ParentMenuId;
            while (parentId.HasValue)
            {
                if (visibleIds.Contains(parentId.Value))
                    break;

                var parent = allEnabled.FirstOrDefault(m => m.MenuId == parentId.Value);
                if (parent is null)
                    break;

                result.Add(new MenuDto
                {
                    MenuId = parent.MenuId,
                    MenuCode = parent.MenuCode,
                    MenuName = parent.MenuName,
                    ParentMenuId = parent.ParentMenuId,
                    Route = parent.Route,
                    Icon = parent.Icon,
                    SortOrder = parent.SortOrder,
                    IsEnabled = parent.IsEnabled
                });
                visibleIds.Add(parent.MenuId);
                parentId = parent.ParentMenuId;
            }
        }

        return result;
    }
}

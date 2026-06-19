namespace Gym.Application.Interfaces;

using Gym.Application.DTOs.Menus;

public interface IGymMenuRepository
{
    Task<IReadOnlyList<MenuDto>> GetAllMenusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEnabledMenuCodesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<bool> IsMenuEnabledAsync(Guid gymId, string menuCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantMenuDto>> GetGymMenusAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task SetMenuEnabledAsync(Guid gymId, int menuId, bool isEnabled, Guid? enabledBy, CancellationToken cancellationToken = default);
    Task BulkSetMenusEnabledAsync(Guid gymId, IReadOnlyList<int> menuIds, bool isEnabled, Guid? enabledBy, CancellationToken cancellationToken = default);
    Task SeedMenusForGymAsync(Guid gymId, Guid? enabledBy, CancellationToken cancellationToken = default);
}

namespace Gym.Application.Interfaces;

using Gym.Application.DTOs.Menus;

public interface IGymMenuService
{
    Task<MyMenusResponseDto> GetMyMenusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuDto>> GetAllMenusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantMenuDto>> GetGymMenusAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymMenuSummaryDto>> GetGymSummariesAsync(CancellationToken cancellationToken = default);
    Task SetMenuEnabledAsync(Guid gymId, int menuId, bool isEnabled, CancellationToken cancellationToken = default);
    Task BulkSetMenusEnabledAsync(Guid gymId, BulkSetGymMenusDto dto, CancellationToken cancellationToken = default);
    Task<bool> IsMenuEnabledAsync(Guid gymId, string menuCode, CancellationToken cancellationToken = default);
    Task SeedMenusForGymAsync(Guid gymId, Guid? enabledBy, CancellationToken cancellationToken = default);
    void InvalidateCache(Guid gymId);
}

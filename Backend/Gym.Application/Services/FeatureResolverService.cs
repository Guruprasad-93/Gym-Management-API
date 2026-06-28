using Gym.Application.Authorization;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class FeatureResolverService : IFeatureResolverService
{
    private readonly IFeatureRepository _featureRepository;
    private readonly IGymMenuRepository _gymMenuRepository;
    private readonly ICurrentUserService _currentUser;
    private IReadOnlyList<FeatureApiRouteDto>? _apiRoutesCache;

    public FeatureResolverService(
        IFeatureRepository featureRepository,
        IGymMenuRepository gymMenuRepository,
        ICurrentUserService currentUser)
    {
        _featureRepository = featureRepository;
        _gymMenuRepository = gymMenuRepository;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<string>> GetSubscriptionFeatureCodesAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _featureRepository.GetEnabledFeatureCodesAsync(gymId, cancellationToken);

    public Task<IReadOnlyList<string>> GetVisibleMenuCodesAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _featureRepository.GetVisibleMenuCodesAsync(gymId, cancellationToken);

    public async Task<IReadOnlyList<string>> GetAccessibleMenuCodesAsync(
        Guid gymId,
        IEnumerable<string> userPermissions,
        CancellationToken cancellationToken = default)
    {
        var visible = await GetVisibleMenuCodesAsync(gymId, cancellationToken);
        return visible
            .Where(code => MenuPermissionMap.UserCanSeeMenu(code, userPermissions))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetEnabledFeatureCodesAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var subscriptionFeatures = await GetSubscriptionFeatureCodesAsync(gymId, cancellationToken);
        var visibleMenus = (await GetVisibleMenuCodesAsync(gymId, cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var enabled = new List<string>();
        foreach (var featureCode in subscriptionFeatures)
        {
            if (await IsFeatureEnabledForGymAsync(gymId, featureCode, visibleMenus, cancellationToken))
                enabled.Add(featureCode);
        }

        return enabled;
    }

    public async Task<bool> HasFeatureAsync(Guid gymId, string featureCode, CancellationToken cancellationToken = default)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
            return true;

        var subscriptionFeatures = await GetSubscriptionFeatureCodesAsync(gymId, cancellationToken);
        if (!subscriptionFeatures.Contains(featureCode, StringComparer.OrdinalIgnoreCase))
            return false;

        var visibleMenus = (await GetVisibleMenuCodesAsync(gymId, cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return await IsFeatureEnabledForGymAsync(gymId, featureCode, visibleMenus, cancellationToken);
    }

    public async Task<bool> IsMenuVisibleAsync(Guid gymId, string menuCode, CancellationToken cancellationToken = default)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
            return true;

        var visible = await GetVisibleMenuCodesAsync(gymId, cancellationToken);
        return visible.Contains(menuCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string?> ResolveFeatureCodeForPathAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        _apiRoutesCache ??= await _featureRepository.GetApiRoutesAsync(cancellationToken);
        var normalized = path.ToLowerInvariant();

        foreach (var route in _apiRoutesCache)
        {
            if (normalized.StartsWith(route.RoutePrefix.ToLowerInvariant(), StringComparison.Ordinal))
                return route.FeatureCode;
        }

        return ApiRouteFeatureMap.ResolveFeatureCode(path);
    }

    private async Task<bool> IsFeatureEnabledForGymAsync(
        Guid gymId,
        string featureCode,
        HashSet<string> visibleMenus,
        CancellationToken cancellationToken)
    {
        var menuCodes = await _featureRepository.GetMenuCodesForFeatureAsync(featureCode, cancellationToken);
        if (menuCodes.Count == 0)
            return true;

        return menuCodes.Any(code => visibleMenus.Contains(code));
    }
}

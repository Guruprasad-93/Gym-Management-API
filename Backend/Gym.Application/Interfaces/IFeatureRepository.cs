using Gym.Application.DTOs.Saas;

namespace Gym.Application.Interfaces;

public interface IFeatureRepository
{
    Task<IReadOnlyList<SystemFeatureDto>> GetAllFeaturesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEnabledFeatureCodesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetVisibleMenuCodesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetMenuCodesForFeatureAsync(string featureCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureApiRouteDto>> GetApiRoutesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureDependencyDto>> GetFeatureDependenciesAsync(CancellationToken cancellationToken = default);
}

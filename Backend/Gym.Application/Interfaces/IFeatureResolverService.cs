namespace Gym.Application.Interfaces;

public interface IFeatureResolverService
{
    Task<IReadOnlyList<string>> GetSubscriptionFeatureCodesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetVisibleMenuCodesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAccessibleMenuCodesAsync(Guid gymId, IEnumerable<string> userPermissions, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEnabledFeatureCodesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<bool> HasFeatureAsync(Guid gymId, string featureCode, CancellationToken cancellationToken = default);
    Task<bool> IsMenuVisibleAsync(Guid gymId, string menuCode, CancellationToken cancellationToken = default);
    Task<string?> ResolveFeatureCodeForPathAsync(string path, CancellationToken cancellationToken = default);
}

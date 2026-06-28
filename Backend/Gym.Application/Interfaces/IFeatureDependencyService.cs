using Gym.Application.Authorization;

namespace Gym.Application.Interfaces;

public interface IFeatureDependencyService
{
    Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetDependencyMapAsync(CancellationToken cancellationToken = default);
    Task<FeatureDependencyValidationResult> ValidateFeatureSelectionAsync(
        IEnumerable<string> featureCodes,
        CancellationToken cancellationToken = default);
    Task ValidateFeatureSelectionOrThrowAsync(
        IReadOnlyList<int> featureIds,
        CancellationToken cancellationToken = default);
}

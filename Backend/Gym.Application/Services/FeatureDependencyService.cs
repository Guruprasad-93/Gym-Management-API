using Gym.Application.Authorization;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class FeatureDependencyService : IFeatureDependencyService
{
    private readonly IFeatureRepository _featureRepository;

    public FeatureDependencyService(IFeatureRepository featureRepository) =>
        _featureRepository = featureRepository;

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetDependencyMapAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _featureRepository.GetFeatureDependenciesAsync(cancellationToken);
        if (rows.Count == 0)
            return FeatureDependencyRules.DefaultDependencies;

        return rows
            .GroupBy(r => r.FeatureCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(x => x.RequiresFeatureCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.OrdinalIgnoreCase);
    }

    public async Task<FeatureDependencyValidationResult> ValidateFeatureSelectionAsync(
        IEnumerable<string> featureCodes,
        CancellationToken cancellationToken = default)
    {
        var map = await GetDependencyMapAsync(cancellationToken);
        return FeatureDependencyRules.Validate(featureCodes, map);
    }

    public async Task ValidateFeatureSelectionOrThrowAsync(
        IReadOnlyList<int> featureIds,
        CancellationToken cancellationToken = default)
    {
        if (featureIds.Count == 0)
            return;

        var allFeatures = await _featureRepository.GetAllFeaturesAsync(cancellationToken: cancellationToken);
        var codeById = allFeatures.ToDictionary(f => f.FeatureId, f => f.FeatureCode);
        var selectedCodes = featureIds
            .Where(id => codeById.ContainsKey(id))
            .Select(id => codeById[id])
            .ToList();

        var result = await ValidateFeatureSelectionAsync(selectedCodes, cancellationToken);
        if (result.IsValid)
            return;

        var messages = string.Join(' ', result.Violations.Select(v => v.Message));
        throw new InvalidOperationException(messages);
    }
}

namespace Gym.Application.Authorization;

/// <summary>
/// Subscription feature dependency rules. DB table FeatureDependencies is the source of truth at runtime;
/// this class provides the default catalog and validation helpers.
/// MULTI_BRANCH requires MEMBERS + TRAINERS (branch management operational baseline).
/// </summary>
public static class FeatureDependencyRules
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultDependencies =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["WEBSITE_BUILDER"] = ["PUBLIC_WEBSITE"],
            ["AI_INSIGHTS"] = ["REPORTS"],
            ["MULTI_BRANCH"] = ["MEMBERS", "TRAINERS"],
        };

    public static FeatureDependencyValidationResult Validate(
        IEnumerable<string> selectedFeatureCodes,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? dependencyMap = null)
    {
        var map = dependencyMap ?? DefaultDependencies;
        var selected = selectedFeatureCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var violations = new List<FeatureDependencyViolation>();

        foreach (var featureCode in selected)
        {
            if (!map.TryGetValue(featureCode, out var requiredCodes))
                continue;

            foreach (var required in requiredCodes)
            {
                if (!selected.Contains(required))
                {
                    violations.Add(new FeatureDependencyViolation
                    {
                        FeatureCode = featureCode,
                        RequiresFeatureCode = required,
                        Message = $"Feature '{featureCode}' requires '{required}' to be included."
                    });
                }
            }
        }

        return new FeatureDependencyValidationResult
        {
            IsValid = violations.Count == 0,
            Violations = violations
        };
    }
}

public sealed class FeatureDependencyValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<FeatureDependencyViolation> Violations { get; init; } = Array.Empty<FeatureDependencyViolation>();
}

public sealed class FeatureDependencyViolation
{
    public string FeatureCode { get; init; } = string.Empty;
    public string RequiresFeatureCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

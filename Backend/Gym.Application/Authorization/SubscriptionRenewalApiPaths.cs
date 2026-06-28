namespace Gym.Application.Authorization;

public static class SubscriptionRenewalApiPaths
{
    private static readonly string[] AuthPrefixes =
    [
        "/api/auth/"
    ];

    private static readonly string[] RenewalPrefixes =
    [
        "/api/saas/subscription",
        "/api/saas/usage",
        "/api/saas/plans",
        "/api/saas/payments/",
        "/api/white-label/app-branding",
        "/api/menus/my-menus",
        "/api/subscription-notifications/"
    ];

    public static bool IsAuthPath(string? path) =>
        AuthPrefixes.Any(prefix => Normalize(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    public static bool IsRenewalPath(string? path)
    {
        var normalized = Normalize(path);
        if (IsAuthPath(path))
            return true;

        if (string.Equals(normalized, "/health", StringComparison.OrdinalIgnoreCase))
            return true;

        return RenewalPrefixes.Any(prefix => normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string? path)
    {
        var value = path ?? string.Empty;
        if (!value.StartsWith('/'))
            value = "/" + value;

        return value.TrimEnd('/');
    }
}

using Gym.Application.Authorization;

namespace Gym.Application.Authorization;

/// <summary>
/// Maps API route prefixes to subscription feature codes (C# fallback when DB routes are unavailable).
/// </summary>
public static class ApiRouteFeatureMap
{
    private static readonly (string Prefix, string FeatureCode)[] Routes =
    [
        ("/api/dashboard", "DASHBOARD"),
        ("/api/analytics", "REPORTS"),
        ("/api/leads", "CRM"),
        ("/api/members", "MEMBERS"),
        ("/api/membership-plans", "MEMBERSHIPS"),
        ("/api/memberships", "MEMBERSHIPS"),
        ("/api/payments", "PAYMENTS"),
        ("/api/revenue", "PAYMENTS"),
        ("/api/attendance", "ATTENDANCE"),
        ("/api/diet-plans", "DIET_PLANS"),
        ("/api/workout-plans", "WORKOUT_PLANS"),
        ("/api/notifications", "NOTIFICATIONS"),
        ("/api/mobile-notifications", "NOTIFICATIONS"),
        ("/api/mobile", "NOTIFICATIONS"),
        ("/api/expenses", "REPORTS"),
        ("/api/payroll", "REPORTS"),
        ("/api/financial", "REPORTS"),
        ("/api/trainers", "TRAINERS"),
        ("/api/branches", "MULTI_BRANCH"),
        ("/api/bookings", "BOOKINGS"),
        ("/api/schedules", "BOOKINGS"),
        ("/api/booking-analytics", "BOOKINGS"),
        ("/api/trainer-schedule", "BOOKINGS"),
        ("/api/booking-checkin", "BOOKINGS"),
        ("/api/ai", "AI_INSIGHTS"),
        ("/api/website", "WEBSITE_BUILDER"),
        ("/api/white-label", "WHITE_LABEL"),
        ("/api/saas", "SUBSCRIPTIONS"),
    ];

    private static readonly string[] ExcludedPrefixes =
    [
        "/api/auth",
        "/api/menus",
        "/api/onboarding",
        "/api/public",
        "/api/health",
        "/api/gyms",
        "/api/roles",
        "/api/privileges",
        "/api/users",
        "/api/platform/tenant-menus",
        "/api/saas/platform",
        "/api/platform/subscription-plans",
        "/api/platform/white-label",
    ];

    public static string? ResolveFeatureCode(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var normalized = path.ToLowerInvariant();
        foreach (var excluded in ExcludedPrefixes)
        {
            if (normalized.StartsWith(excluded, StringComparison.Ordinal))
                return null;
        }

        foreach (var (prefix, featureCode) in Routes)
        {
            if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                return featureCode;
        }

        return null;
    }
}

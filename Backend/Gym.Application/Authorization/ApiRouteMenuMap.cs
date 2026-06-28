using System.Text.RegularExpressions;
using Gym.Application.Constants;

namespace Gym.Application.Authorization;

/// <summary>
/// Maps API route prefixes to tenant menu module codes for backend enforcement.
/// </summary>
public static class ApiRouteMenuMap
{
    private static readonly (string Prefix, string MenuCode)[] Routes =
    [
        ("/api/dashboard", MenuCodes.Dashboard),
        ("/api/analytics/revenue", MenuCodes.RevenueAnalytics),
        ("/api/analytics/members", MenuCodes.MemberAnalytics),
        ("/api/analytics/attendance", MenuCodes.AttendanceAnalytics),
        ("/api/analytics/trainers", MenuCodes.TrainerAnalytics),
        ("/api/analytics", MenuCodes.Analytics),
        ("/api/leads", MenuCodes.Leads),
        ("/api/members", MenuCodes.Members),
        ("/api/membership-plans", MenuCodes.MembershipPlans),
        ("/api/memberships", MenuCodes.Memberships),
        ("/api/payments", MenuCodes.Payments),
        ("/api/revenue", MenuCodes.Revenue),
        ("/api/attendance", MenuCodes.Attendance),
        ("/api/audit-logs", MenuCodes.Reports),
        ("/api/diet-plans", MenuCodes.DietPlans),
        ("/api/workout-plans", MenuCodes.WorkoutPlans),
        ("/api/notifications", MenuCodes.Notifications),
        ("/api/mobile-notifications", MenuCodes.MobilePush),
        ("/api/mobile", MenuCodes.MobilePush),
        ("/api/expenses", MenuCodes.Financial),
        ("/api/payroll", MenuCodes.Financial),
        ("/api/financial", MenuCodes.Financial),
        ("/api/trainers", MenuCodes.Trainers),
        ("/api/gym-admins", MenuCodes.Staff),
        ("/api/branches", MenuCodes.Branches),
        ("/api/ai", MenuCodes.AiInsights),
        ("/api/bookings", MenuCodes.Bookings),
        ("/api/schedules", MenuCodes.ClassSchedules),
        ("/api/booking-analytics", MenuCodes.BookingAnalytics),
        ("/api/trainer-schedule", MenuCodes.Bookings),
        ("/api/booking-checkin", MenuCodes.Bookings),
        ("/api/website", MenuCodes.WebsiteBuilder),
        ("/api/white-label", MenuCodes.WhiteLabel),
        ("/api/saas", MenuCodes.Subscriptions),
        ("/api/files/gym", MenuCodes.GymBranding),
        ("/api/files/members", MenuCodes.Members),
        ("/api/files/trainers", MenuCodes.Trainers),
        ("/api/files", MenuCodes.Inventory),
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
        "/api/platform/white-label",
        "/api/files/upload",
    ];

    private static readonly Regex FileContentDownloadPath = new(
        @"^/api/files/\d+/content(?:/|$|\?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? ResolveMenuCode(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var normalized = path.ToLowerInvariant();

        // Signed file downloads are AllowAnonymous; exclude from tenant menu checks so
        // <img src="/api/files/{id}/content?..."> works while the user is logged in.
        if (FileContentDownloadPath.IsMatch(normalized))
            return null;

        foreach (var excluded in ExcludedPrefixes)
        {
            if (normalized.StartsWith(excluded, StringComparison.Ordinal))
                return null;
        }

        if (normalized.StartsWith("/api/white-label/app-branding", StringComparison.Ordinal))
            return null;

        foreach (var (prefix, menuCode) in Routes)
        {
            if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                return menuCode;
        }

        return null;
    }
}

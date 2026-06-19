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
        ("/api/audit-logs", MenuCodes.AuditLogs),
        ("/api/diet-plans", MenuCodes.DietPlans),
        ("/api/workout-plans", MenuCodes.WorkoutPlans),
        ("/api/notifications", MenuCodes.Notifications),
        ("/api/mobile-notifications", MenuCodes.MobilePush),
        ("/api/mobile", MenuCodes.MobilePush),
        ("/api/expenses", MenuCodes.Expenses),
        ("/api/payroll", MenuCodes.Payroll),
        ("/api/financial", MenuCodes.Financial),
        ("/api/trainers", MenuCodes.Trainers),
        ("/api/gym-admins", MenuCodes.Staff),
        ("/api/branches", MenuCodes.Branches),
        ("/api/ai", MenuCodes.AiInsights),
        ("/api/bookings", MenuCodes.Bookings),
        ("/api/schedules", MenuCodes.ClassSchedules),
        ("/api/booking-analytics", MenuCodes.BookingAnalytics),
        ("/api/website", MenuCodes.WebsiteBuilder),
        ("/api/white-label", MenuCodes.WhiteLabel),
        ("/api/saas", MenuCodes.Subscriptions),
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
    ];

    public static string? ResolveMenuCode(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var normalized = path.ToLowerInvariant();
        foreach (var excluded in ExcludedPrefixes)
        {
            if (normalized.StartsWith(excluded, StringComparison.Ordinal))
                return null;
        }

        foreach (var (prefix, menuCode) in Routes)
        {
            if (normalized.StartsWith(prefix, StringComparison.Ordinal))
                return menuCode;
        }

        return null;
    }
}

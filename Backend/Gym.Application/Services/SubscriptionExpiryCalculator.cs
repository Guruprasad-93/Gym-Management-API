using Gym.Application.Constants;
using Gym.Application.DTOs.Saas;

namespace Gym.Application.Services;

public sealed class SubscriptionExpiryMilestone
{
    public string NotificationType { get; init; } = string.Empty;
    public string NotificationKey { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = SubscriptionNotificationSeverity.Info;
    public bool ShowLoginPopup { get; init; }
}

public static class SubscriptionExpiryCalculator
{
    public static int? ComputeDaysToExpiry(DateTime today, DateTime? currentPeriodEnd)
    {
        if (currentPeriodEnd is null)
            return null;

        var days = (currentPeriodEnd.Value.Date - today.Date).Days;
        return days < 0 ? 0 : days;
    }

    public static int? ComputeGraceDaysRemaining(DateTime today, DateTime? graceEndsAt)
    {
        if (graceEndsAt is null)
            return null;

        var days = (graceEndsAt.Value.Date - today.Date).Days;
        return Math.Max(0, days);
    }

    public static bool IsInGracePeriod(GymSubscriptionDto subscription, DateTime today)
    {
        if (subscription.GraceEndsAt is null || subscription.GraceEndsAt.Value.Date < today.Date)
            return false;

        var periodEnded = subscription.CurrentPeriodEnd.HasValue
            && subscription.CurrentPeriodEnd.Value.Date < today.Date;
        var trialEnded = string.Equals(subscription.Status, "Trial", StringComparison.OrdinalIgnoreCase)
            && subscription.TrialEndsAt.HasValue
            && subscription.TrialEndsAt.Value.Date < today.Date;

        return periodEnded || trialEnded;
    }

    public static SubscriptionExpiryMilestone? ResolveDailyMilestone(
        GymSubscriptionDto subscription,
        DateTime todayUtc)
    {
        var today = todayUtc.Date;
        var periodEnd = subscription.CurrentPeriodEnd?.Date;
        var graceEnd = subscription.GraceEndsAt?.Date;
        var subscriptionId = subscription.Id;

        if (IsInGracePeriod(subscription, today))
        {
            var graceDays = ComputeGraceDaysRemaining(today, graceEnd);
            return graceDays switch
            {
                2 => new SubscriptionExpiryMilestone
                {
                    NotificationType = SubscriptionNotificationTypes.Grace2DaysRemaining,
                    NotificationKey = $"sub-{subscriptionId}-grace-2-{graceEnd:yyyyMMdd}",
                    Title = "Grace Period",
                    Message = "Grace period remaining: 2 days.",
                    Severity = SubscriptionNotificationSeverity.Warning
                },
                1 => new SubscriptionExpiryMilestone
                {
                    NotificationType = SubscriptionNotificationTypes.Grace1DayRemaining,
                    NotificationKey = $"sub-{subscriptionId}-grace-1-{graceEnd:yyyyMMdd}",
                    Title = "Grace Period",
                    Message = "Grace period remaining: 1 day.",
                    Severity = SubscriptionNotificationSeverity.Warning
                },
                0 when graceEnd == today => new SubscriptionExpiryMilestone
                {
                    NotificationType = SubscriptionNotificationTypes.GraceLastDay,
                    NotificationKey = $"sub-{subscriptionId}-grace-last-{graceEnd:yyyyMMdd}",
                    Title = "Grace Period Ending",
                    Message = "Last day of grace period. Renew now.",
                    Severity = SubscriptionNotificationSeverity.Critical,
                    ShowLoginPopup = true
                },
                _ => null
            };
        }

        if (periodEnd is null || periodEnd < today)
            return null;

        var daysToExpiry = ComputeDaysToExpiry(today, periodEnd);
        return daysToExpiry switch
        {
            7 => BuildPreExpiryMilestone(subscriptionId, periodEnd.Value, 7,
                SubscriptionNotificationTypes.Expiry7Days,
                "Your subscription expires in 7 days."),
            3 => BuildPreExpiryMilestone(subscriptionId, periodEnd.Value, 3,
                SubscriptionNotificationTypes.Expiry3Days,
                "Your subscription expires in 3 days."),
            2 => BuildPreExpiryMilestone(subscriptionId, periodEnd.Value, 2,
                SubscriptionNotificationTypes.Expiry2Days,
                "Your subscription expires in 2 days. Renew now to avoid interruption.",
                SubscriptionNotificationSeverity.Warning, showPopup: true),
            1 => BuildPreExpiryMilestone(subscriptionId, periodEnd.Value, 1,
                SubscriptionNotificationTypes.Expiry1Day,
                "Your subscription expires tomorrow.",
                SubscriptionNotificationSeverity.Critical, showPopup: true),
            0 => new SubscriptionExpiryMilestone
            {
                NotificationType = SubscriptionNotificationTypes.ExpiredToday,
                NotificationKey = $"sub-{subscriptionId}-expired-{periodEnd:yyyyMMdd}",
                Title = "Subscription Expired",
                Message = BuildExpiredTodayMessage(subscription.GraceEndsAt, today),
                Severity = SubscriptionNotificationSeverity.Critical,
                ShowLoginPopup = true
            },
            _ => null
        };
    }

    public static string? ResolveBannerMessage(
        string accessMode,
        int? daysToExpiry,
        int? graceDaysRemaining,
        DateTime? graceEndsAt = null,
        DateTime? today = null)
    {
        var todayDate = (today ?? DateTime.UtcNow).Date;

        if (accessMode == SubscriptionAccessModes.GracePeriod)
        {
            return graceDaysRemaining switch
            {
                2 => "Grace period remaining: 2 days.",
                1 => "Grace period remaining: 1 day.",
                0 => "Last day of grace period. Renew now.",
                _ => graceDaysRemaining is > 0
                    ? $"Grace period remaining: {graceDaysRemaining} days."
                    : "Last day of grace period. Renew now."
            };
        }

        if (accessMode != SubscriptionAccessModes.Active)
            return null;

        if (daysToExpiry == 0)
            return BuildExpiredTodayMessage(graceEndsAt, todayDate);

        return daysToExpiry switch
        {
            7 => "Your subscription expires in 7 days.",
            3 => "Your subscription expires in 3 days.",
            2 => "Your subscription expires in 2 days. Renew now to avoid interruption.",
            1 => "Your subscription expires tomorrow.",
            _ => null
        };
    }

    public static string ResolveBannerPriority(string accessMode, int? daysToExpiry, int? graceDaysRemaining)
    {
        if (accessMode == SubscriptionAccessModes.GracePeriod)
        {
            if (graceDaysRemaining is 0 or 1)
                return SubscriptionNotificationSeverity.Critical;
            return SubscriptionNotificationSeverity.Warning;
        }

        return daysToExpiry switch
        {
            1 or 2 => SubscriptionNotificationSeverity.Critical,
            3 or 7 => SubscriptionNotificationSeverity.Warning,
            0 => SubscriptionNotificationSeverity.Critical,
            _ => SubscriptionNotificationSeverity.Info
        };
    }

    private static string BuildExpiredTodayMessage(DateTime? graceEndsAt, DateTime today)
    {
        var graceDays = ComputeGraceDaysRemaining(today, graceEndsAt) ?? 0;
        return graceDays > 0
            ? $"Your subscription has expired. Grace period remaining: {graceDays} days."
            : "Your subscription has expired.";
    }

    private static SubscriptionExpiryMilestone BuildPreExpiryMilestone(
        int subscriptionId,
        DateTime periodEnd,
        int days,
        string type,
        string message,
        string severity = SubscriptionNotificationSeverity.Info,
        bool showPopup = false) =>
        new()
        {
            NotificationType = type,
            NotificationKey = $"sub-{subscriptionId}-expiry-{days}-{periodEnd:yyyyMMdd}",
            Title = "Subscription Reminder",
            Message = message,
            Severity = severity,
            ShowLoginPopup = showPopup
        };
}

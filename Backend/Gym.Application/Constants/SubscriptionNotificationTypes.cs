namespace Gym.Application.Constants;

public static class SubscriptionNotificationTypes
{
    public const string Expiry7Days = "SubscriptionExpiry7Days";
    public const string Expiry3Days = "SubscriptionExpiry3Days";
    public const string Expiry2Days = "SubscriptionExpiry2Days";
    public const string Expiry1Day = "SubscriptionExpiry1Day";
    public const string ExpiredToday = "SubscriptionExpiredToday";
    public const string Grace2DaysRemaining = "SubscriptionGrace2Days";
    public const string Grace1DayRemaining = "SubscriptionGrace1Day";
    public const string GraceLastDay = "SubscriptionGraceLastDay";
}

public static class SubscriptionNotificationSeverity
{
    public const string Info = "Info";
    public const string Warning = "Warning";
    public const string Critical = "Critical";
}

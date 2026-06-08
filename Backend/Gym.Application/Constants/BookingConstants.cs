namespace Gym.Application.Constants;

public static class BookingStatuses
{
    public const string Booked = "Booked";
    public const string CheckedIn = "CheckedIn";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";
}

public static class ClassScheduleStatuses
{
    public const string Active = "Active";
    public const string Cancelled = "Cancelled";
}

public static class BookingNotificationTypes
{
    public const string BookingCreated = "BookingCreated";
    public const string BookingReminder = "BookingReminder";
    public const string BookingCancelled = "BookingCancelled";
    public const string WaitlistPromoted = "WaitlistPromoted";
    public const string TrainerAssignment = "TrainerAssignment";
}

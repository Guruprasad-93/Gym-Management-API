namespace Gym.Application.Constants;

public static class LeadStatuses
{
    public const string New = "New";
    public const string Contacted = "Contacted";
    public const string TrialScheduled = "TrialScheduled";
    public const string TrialCompleted = "TrialCompleted";
    public const string FollowUpPending = "FollowUpPending";
    public const string Converted = "Converted";
    public const string Lost = "Lost";

    public static readonly IReadOnlyList<string> All =
    [
        New, Contacted, TrialScheduled, TrialCompleted, FollowUpPending, Converted, Lost
    ];
}

public static class LeadSources
{
    public const string WalkIn = "WalkIn";
    public const string Referral = "Referral";
    public const string Facebook = "Facebook";
    public const string Instagram = "Instagram";
    public const string Google = "Google";
    public const string Website = "Website";
    public const string WhatsApp = "WhatsApp";
    public const string Other = "Other";

    public static readonly IReadOnlyList<string> All =
    [
        WalkIn, Referral, Facebook, Instagram, Google, Website, WhatsApp, Other
    ];
}

public static class LeadActivityTypes
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string StatusChanged = "StatusChanged";
    public const string TrainerAssigned = "TrainerAssigned";
    public const string TrialScheduled = "TrialScheduled";
    public const string TrialCompleted = "TrialCompleted";
    public const string FollowUpCreated = "FollowUpCreated";
    public const string Converted = "Converted";
    public const string MarkedLost = "MarkedLost";
}

public static class LeadFollowUpStatuses
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class LeadTrialAttendanceStatuses
{
    public const string Scheduled = "Scheduled";
    public const string Attended = "Attended";
    public const string NoShow = "NoShow";
    public const string Cancelled = "Cancelled";
}

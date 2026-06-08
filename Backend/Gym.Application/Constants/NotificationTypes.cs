namespace Gym.Application.Constants;

public static class NotificationTypes
{
    public const string MembershipExpiry7Days = "MembershipExpiry7Days";
    public const string MembershipExpiry3Days = "MembershipExpiry3Days";
    public const string MembershipExpiryToday = "MembershipExpiryToday";
    public const string PaymentSuccess = "PaymentSuccess";
    public const string MembershipRenewal = "MembershipRenewal";
    public const string NewMemberRegistration = "NewMemberRegistration";
    public const string WorkoutPlanAssigned = "WorkoutPlanAssigned";
    public const string DietPlanAssigned = "DietPlanAssigned";
    public const string GymOwnerWelcome = "GymOwnerWelcome";
    public const string LeadCreated = "LeadCreated";
    public const string TrialScheduled = "TrialScheduled";
    public const string TrialReminder = "TrialReminder";
    public const string FollowUpReminder = "FollowUpReminder";
    public const string LeadConverted = "LeadConverted";
    public const string GoalCompleted = "GoalCompleted";
    public const string ReferralRewardEarned = "ReferralRewardEarned";
    public const string BranchAnnouncement = "BranchAnnouncement";
    public const string WebsiteLeadCreated = "WebsiteLeadCreated";
    public const string TrialBooked = "TrialBooked";

    public static readonly IReadOnlyList<string> All =
    [
        MembershipExpiry7Days,
        MembershipExpiry3Days,
        MembershipExpiryToday,
        PaymentSuccess,
        MembershipRenewal,
        NewMemberRegistration,
        WorkoutPlanAssigned,
        DietPlanAssigned,
        GymOwnerWelcome,
        LeadCreated,
        TrialScheduled,
        TrialReminder,
        FollowUpReminder,
        LeadConverted,
        GoalCompleted,
        ReferralRewardEarned,
        BranchAnnouncement,
        WebsiteLeadCreated,
        TrialBooked
    ];
}

public static class NotificationStatuses
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}

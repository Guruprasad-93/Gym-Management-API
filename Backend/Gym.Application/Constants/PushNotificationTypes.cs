namespace Gym.Application.Constants;

public static class PushNotificationTypes
{
    public const string ManualCampaign = "ManualCampaign";
    public const string MembershipExpiry7Days = "MembershipExpiry7Days";
    public const string MembershipExpiry3Days = "MembershipExpiry3Days";
    public const string MembershipExpiryToday = "MembershipExpiryToday";
    public const string PaymentSuccess = "PaymentSuccess";
    public const string MembershipRenewal = "MembershipRenewal";
    public const string WorkoutPlanAssigned = "WorkoutPlanAssigned";
    public const string WorkoutReminder = "WorkoutReminder";
    public const string DietPlanAssigned = "DietPlanAssigned";
    public const string DietReminder = "DietReminder";
    public const string AttendanceReminder = "AttendanceReminder";
    public const string GoalCompleted = "GoalCompleted";
    public const string GoalReminder = "GoalReminder";
    public const string ReferralRewardEarned = "ReferralRewardEarned";
    public const string BranchAnnouncement = "BranchAnnouncement";
    public const string LeadAssigned = "LeadAssigned";
    public const string RenewalRiskAlert = "RenewalRiskAlert";
    public const string ChurnRiskAlert = "ChurnRiskAlert";
    public const string BookingCreated = "BookingCreated";
    public const string BookingReminder = "BookingReminder";
    public const string BookingCancelled = "BookingCancelled";
    public const string WaitlistPromoted = "WaitlistPromoted";
    public const string TrainerAssignment = "TrainerAssignment";
}

public static class PushNotificationStatuses
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Delivered = "Delivered";
    public const string Failed = "Failed";
    public const string Opened = "Opened";
    public const string Clicked = "Clicked";
}

public static class DeviceTypes
{
    public const string Android = "Android";
    public const string Ios = "iOS";
}

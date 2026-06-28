namespace Gym.Application.Constants;

public static class SaasSubscriptionStatuses
{
    public const string Trial = "Trial";
    public const string Active = "Active";
    public const string PastDue = "PastDue";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
}

public static class SaasBillingCycles
{
    public const string Trial = "Trial";
    public const string Monthly = "Monthly";
    public const string Quarterly = "Quarterly";
    public const string HalfYearly = "HalfYearly";
    public const string Yearly = "Yearly";
}

public static class SaasPlanCodes
{
    public const string Trial = "Trial";
    public const string Basic = "Basic";
    public const string Premium = "Premium";
    public const string Enterprise = "PremiumPro";
    public const string PremiumPro = "PremiumPro";
}

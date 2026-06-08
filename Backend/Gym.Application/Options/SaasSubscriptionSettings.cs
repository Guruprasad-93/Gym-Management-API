namespace Gym.Application.Options;

public class SaasSubscriptionSettings
{
    public const string SectionName = "SaasSubscription";

    public int TrialDays { get; set; } = 15;
    public int GracePeriodDays { get; set; } = 3;
    public bool AllowPublicRegistration { get; set; } = true;
}

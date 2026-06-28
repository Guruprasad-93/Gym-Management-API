namespace Gym.Application.DTOs.Auth;

using Gym.Application.Constants;

public class SubscriptionAccessStateDto
{
    public string AccessMode { get; set; } = Constants.SubscriptionAccessModes.Active;
    public bool HasSubscriptionAccess { get; set; } = true;
    public DateTime? GraceEndsAt { get; set; }
    public int? GraceDaysRemaining { get; set; }
    public int? DaysToExpiry { get; set; }
    public string? BannerMessage { get; set; }
    public string? BannerSeverity { get; set; }
}

namespace Gym.Application.DTOs.Auth;

using Gym.Application.Constants;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public string? GymName { get; set; }
    public Guid SessionId { get; set; }
    public int TokenVersion { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> EnabledMenuCodes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> EnabledFeatureCodes { get; set; } = Array.Empty<string>();
    public bool MustChangePassword { get; set; }
    public string SubscriptionAccessMode { get; set; } = Constants.SubscriptionAccessModes.Active;
    public bool HasSubscriptionAccess { get; set; } = true;
    public DateTime? GraceEndsAt { get; set; }
    public int? GraceDaysRemaining { get; set; }
    public int? DaysToExpiry { get; set; }
    public string? BannerMessage { get; set; }
    public string? BannerSeverity { get; set; }
    public bool ShowPoweredBy { get; set; } = true;
    public string? PlatformProductName { get; set; }
}

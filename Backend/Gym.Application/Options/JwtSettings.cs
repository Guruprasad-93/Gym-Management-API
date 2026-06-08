namespace Gym.Application.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "GymManagementSystem";
    public string Audience { get; set; } = "GymManagementSystem";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public int PasswordResetTokenExpiryMinutes { get; set; } = 60;
    /// <summary>When true in Development only, forgot-password may return reset token in API response.</summary>
    public bool ReturnResetTokenInDevelopment { get; set; } = false;
}

namespace Gym.Application.Options;

public class PasswordResetSettings
{
    public const string SectionName = "PasswordReset";

    /// <summary>Angular app base URL for reset links (e.g. http://localhost:4200).</summary>
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";
}

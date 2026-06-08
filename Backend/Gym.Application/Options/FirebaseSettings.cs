namespace Gym.Application.Options;

public class FirebaseSettings
{
    public const string SectionName = "Firebase";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Mock";
    public string? ProjectId { get; set; }
    public string? PrivateKey { get; set; }
    public string? ClientEmail { get; set; }
    public int BackgroundJobIntervalMinutes { get; set; } = 60;
}

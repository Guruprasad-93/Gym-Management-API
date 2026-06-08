namespace Gym.Application.Options;

public class AiSettings
{
    public const string SectionName = "AI";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Mock";
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
    public int BackgroundJobIntervalHours { get; set; } = 24;
    public int MaxMembersPerRun { get; set; } = 200;
}

namespace Gym.Application.Options;

public class ApplicationInsightsSettings
{
    public const string SectionName = "ApplicationInsights";

    public string? ConnectionString { get; set; }
    public bool Enabled { get; set; }
}

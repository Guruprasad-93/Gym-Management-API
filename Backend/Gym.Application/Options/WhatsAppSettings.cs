namespace Gym.Application.Options;

public class WhatsAppSettings
{
    public const string SectionName = "WhatsApp";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Mock";
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultCountryCode { get; set; } = "91";
    public int BackgroundJobIntervalMinutes { get; set; } = 60;
    public int PendingBatchSize { get; set; } = 50;
}

namespace Gym.Application.Options;

public class RazorpaySettings
{
    public const string SectionName = "Razorpay";

    public bool Enabled { get; set; }
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
}

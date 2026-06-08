namespace Gym.Application.Interfaces;

public class WhatsAppTemplateMessage
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    public string LanguageCode { get; set; } = "en";
}

public class WhatsAppSendResult
{
    public bool Success { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IWhatsAppProvider
{
    Task<WhatsAppSendResult> SendTemplateAsync(WhatsAppTemplateMessage message, CancellationToken cancellationToken = default);
}

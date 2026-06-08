using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Services;

public class WhatsAppProvider : IWhatsAppProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HttpClient _httpClient;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<WhatsAppProvider> _logger;

    public WhatsAppProvider(HttpClient httpClient, IOptions<WhatsAppSettings> settings, ILogger<WhatsAppProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WhatsAppSendResult> SendTemplateAsync(
        WhatsAppTemplateMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || string.Equals(_settings.Provider, "Mock", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Mock WhatsApp send to {Phone} template {Template}",
                message.PhoneNumber,
                message.TemplateName);
            return new WhatsAppSendResult
            {
                Success = true,
                ProviderMessageId = $"mock_{Guid.NewGuid():N}"
            };
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiBaseUrl) || string.IsNullOrWhiteSpace(_settings.ApiKey))
            return new WhatsAppSendResult { Success = false, ErrorMessage = "WhatsApp provider is not configured." };

        var payload = BuildProviderPayload(message);
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("WhatsApp send failed: {Status} {Body}", response.StatusCode, body);
            return new WhatsAppSendResult { Success = false, ErrorMessage = body.Length > 500 ? body[..500] : body };
        }

        return new WhatsAppSendResult
        {
            Success = true,
            ProviderMessageId = ExtractMessageId(body)
        };
    }

    private object BuildProviderPayload(WhatsAppTemplateMessage message)
    {
        var phone = NormalizePhone(message.PhoneNumber);
        var bodyValues = message.Variables.Values.ToList();

        return _settings.Provider.ToUpperInvariant() switch
        {
            "AISENSY" => new
            {
                apiKey = _settings.ApiKey,
                campaignName = message.TemplateName,
                destination = phone,
                templateParams = bodyValues
            },
            "WHATSAPPBUSINESS" => new
            {
                messaging_product = "whatsapp",
                to = phone,
                type = "template",
                template = new
                {
                    name = message.TemplateName,
                    language = new { code = message.LanguageCode },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = bodyValues.Select(v => new { type = "text", text = v }).ToArray()
                        }
                    }
                }
            },
            _ => new
            {
                countryCode = $"+{_settings.DefaultCountryCode}",
                phoneNumber = phone.TrimStart('+'),
                type = "Template",
                template = new
                {
                    name = message.TemplateName,
                    languageCode = message.LanguageCode,
                    bodyValues
                }
            }
        };
    }

    private string BuildEndpoint()
    {
        var baseUrl = _settings.ApiBaseUrl.TrimEnd('/');
        return _settings.Provider.ToUpperInvariant() switch
        {
            "AISENSY" => $"{baseUrl}/campaign/t1/api/v2",
            "WHATSAPPBUSINESS" => $"{baseUrl}/messages",
            _ => $"{baseUrl}/message/"
        };
    }

    private string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith(_settings.DefaultCountryCode, StringComparison.Ordinal))
            return $"+{digits}";
        return $"+{_settings.DefaultCountryCode}{digits.TrimStart('0')}";
    }

    private static string? ExtractMessageId(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("id", out var id))
                return id.GetString();
            if (doc.RootElement.TryGetProperty("messageId", out var messageId))
                return messageId.GetString();
        }
        catch
        {
            // ignore parse errors
        }

        return null;
    }
}

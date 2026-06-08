using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Services;

public class RazorpayGateway : IRazorpayGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private readonly HttpClient _httpClient;
    private readonly RazorpaySettings _settings;
    private readonly ILogger<RazorpayGateway> _logger;

    public RazorpayGateway(HttpClient httpClient, IOptions<RazorpaySettings> settings, ILogger<RazorpayGateway> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> CreateOrderAsync(
        decimal amountInRupees,
        string currency,
        string receipt,
        IReadOnlyDictionary<string, string>? notes,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var payload = new Dictionary<string, object>
        {
            ["amount"] = ToPaise(amountInRupees),
            ["currency"] = currency,
            ["receipt"] = receipt
        };

        if (notes is not null && notes.Count > 0)
            payload["notes"] = notes;

        using var request = CreateRequest(HttpMethod.Post, "orders", payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Razorpay order creation failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Unable to create Razorpay order.");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Razorpay order response missing id.");
    }

    public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
    {
        if (string.IsNullOrWhiteSpace(_settings.KeySecret))
            return false;

        var payload = $"{orderId}|{paymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.KeySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    public async Task<string> RefundPaymentAsync(
        string razorpayPaymentId,
        decimal? amountInRupees,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        Dictionary<string, object>? payload = null;
        if (amountInRupees.HasValue)
            payload = new Dictionary<string, object> { ["amount"] = ToPaise(amountInRupees.Value) };

        using var request = CreateRequest(HttpMethod.Post, $"payments/{razorpayPaymentId}/refund", payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Razorpay refund failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Unable to process Razorpay refund.");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Razorpay refund response missing id.");
    }

    private void EnsureConfigured()
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.KeyId) || string.IsNullOrWhiteSpace(_settings.KeySecret))
            throw new InvalidOperationException("Razorpay is not configured.");
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, Dictionary<string, object>? payload)
    {
        var request = new HttpRequestMessage(method, $"https://api.razorpay.com/v1/{path}");
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        if (payload is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        return request;
    }

    private static int ToPaise(decimal amountInRupees) =>
        (int)Math.Round(amountInRupees * 100m, MidpointRounding.AwayFromZero);
}

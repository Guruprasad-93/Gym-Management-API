using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gym.Infrastructure.Services;

public class FirebasePushService : IFirebasePushService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HttpClient _httpClient;
    private readonly FirebaseSettings _settings;
    private readonly ILogger<FirebasePushService> _logger;

    public FirebasePushService(HttpClient httpClient, IOptions<FirebaseSettings> settings, ILogger<FirebasePushService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<FirebasePushResult> SendAsync(FirebasePushMessage message, CancellationToken cancellationToken = default)
    {
        if (string.Equals(_settings.Provider, "Mock", StringComparison.OrdinalIgnoreCase) || !_settings.Enabled)
        {
            _logger.LogInformation("Mock push to {DeviceType}: {Title}", message.DeviceType, message.Title);
            return new FirebasePushResult { Success = true, MessageId = Guid.NewGuid().ToString("N") };
        }

        if (string.IsNullOrWhiteSpace(_settings.ProjectId)
            || string.IsNullOrWhiteSpace(_settings.ClientEmail)
            || string.IsNullOrWhiteSpace(_settings.PrivateKey))
        {
            return new FirebasePushResult { Success = false, ErrorMessage = "Firebase credentials are not configured." };
        }

        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);
            var payload = new
            {
                message = new
                {
                    token = message.DeviceToken,
                    notification = new { title = message.Title, body = message.Body },
                    data = message.Data
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://fcm.googleapis.com/v1/projects/{_settings.ProjectId}/messages:send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("FCM send failed: {Status} {Body}", response.StatusCode, body);
                return new FirebasePushResult { Success = false, ErrorMessage = body };
            }

            using var doc = JsonDocument.Parse(body);
            var messageId = doc.RootElement.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
            return new FirebasePushResult { Success = true, MessageId = messageId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM send exception");
            return new FirebasePushResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var privateKey = _settings.PrivateKey!.Replace("\\n", "\n");
        var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var exp = iat + 3300;

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKey);
        var key = new RsaSecurityKey(rsa);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var jwt = new JwtSecurityToken(
            claims:
            [
                new System.Security.Claims.Claim("iss", _settings.ClientEmail!),
                new System.Security.Claims.Claim("scope", "https://www.googleapis.com/auth/firebase.messaging"),
                new System.Security.Claims.Claim("aud", "https://oauth2.googleapis.com/token"),
                new System.Security.Claims.Claim("iat", iat.ToString(), System.Security.Claims.ClaimValueTypes.Integer64),
                new System.Security.Claims.Claim("exp", exp.ToString(), System.Security.Claims.ClaimValueTypes.Integer64)
            ],
            signingCredentials: creds);

        var assertion = new JwtSecurityTokenHandler().WriteToken(jwt);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = assertion
        });

        var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();
        var json = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("FCM access token missing.");
    }
}

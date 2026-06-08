using System.Security.Cryptography;
using System.Text;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Security;

public class FileDownloadUrlSigner : IFileDownloadUrlSigner
{
    private readonly byte[] _key;
    private readonly int _expiryMinutes;

    public FileDownloadUrlSigner(IOptions<FileStorageSettings> fileStorage, IOptions<JwtSettings> jwt)
    {
        var settings = fileStorage.Value;
        var secret = settings.UrlSigningSecret;
        if (string.IsNullOrWhiteSpace(secret))
            secret = jwt.Value.Secret;

        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
            throw new InvalidOperationException("File URL signing requires FileStorage:UrlSigningSecret or Jwt:Secret (min 32 chars).");

        _key = Encoding.UTF8.GetBytes(secret);
        _expiryMinutes = settings.DownloadUrlExpiryMinutes > 0 ? settings.DownloadUrlExpiryMinutes : 60;
    }

    public string CreateSignedDownloadUrl(long fileId, Guid gymId)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(_expiryMinutes).ToUnixTimeSeconds();
        var sig = ComputeSignature(fileId, gymId, expires);
        return $"/api/files/{fileId}/content?g={gymId:D}&exp={expires}&sig={Uri.EscapeDataString(sig)}";
    }

    public bool TryValidate(long fileId, Guid gymId, long expiresUnixSeconds, string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
            return false;

        if (expiresUnixSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return false;

        var expected = ComputeSignature(fileId, gymId, expiresUnixSeconds);
        var providedBytes = Encoding.UTF8.GetBytes(signature);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return providedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }

    private string ComputeSignature(long fileId, Guid gymId, long expiresUnixSeconds)
    {
        var payload = $"{fileId}|{gymId:D}|{expiresUnixSeconds}";
        using var hmac = new HMACSHA256(_key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }
}

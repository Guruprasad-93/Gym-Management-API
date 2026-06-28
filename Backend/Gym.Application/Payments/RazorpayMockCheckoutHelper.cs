using System.Security.Cryptography;
using System.Text;

namespace Gym.Application.Payments;

/// <summary>
/// Builds verifiable mock payment payloads when <see cref="Options.RazorpaySettings.UseMockGateway"/> is enabled.
/// </summary>
public static class RazorpayMockCheckoutHelper
{
    public const string MockKeyId = "rzp_test_mock";
    public const string DevKeySecret = "mock_razorpay_dev_secret";

    public static bool IsMockOrder(string? orderId) =>
        !string.IsNullOrWhiteSpace(orderId)
        && orderId.StartsWith("order_mock_", StringComparison.OrdinalIgnoreCase);

    public static (string PaymentId, string Signature) CreateSuccess(string orderId, string? keySecret)
    {
        var secret = string.IsNullOrWhiteSpace(keySecret) ? DevKeySecret : keySecret;
        var paymentId = $"pay_mock_{Guid.NewGuid():N}";
        var signature = ComputeSignature(orderId, paymentId, secret);
        return (paymentId, signature);
    }

    public static string ComputeSignature(string orderId, string paymentId, string keySecret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{orderId}|{paymentId}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

using System.Security.Cryptography;
using System.Text;
using Gym.Application.Interfaces;
using Gym.Application.Payments;

namespace Gym.Infrastructure.Services;

/// <summary>Local Razorpay substitute for development/demo — no external API calls.</summary>
public sealed class MockRazorpayGateway : IRazorpayGateway
{
    public const string DevKeySecret = RazorpayMockCheckoutHelper.DevKeySecret;

    private readonly string _keySecret;

    public MockRazorpayGateway(string? keySecret = null) =>
        _keySecret = string.IsNullOrWhiteSpace(keySecret) ? DevKeySecret : keySecret;

    public Task<string> CreateOrderAsync(
        decimal amountInRupees,
        string currency,
        string receipt,
        IReadOnlyDictionary<string, string>? notes,
        CancellationToken cancellationToken = default) =>
        Task.FromResult($"order_mock_{Guid.NewGuid():N}");

    public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
    {
        var expected = RazorpayMockCheckoutHelper.ComputeSignature(orderId, paymentId, _keySecret);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    public Task<string> RefundPaymentAsync(
        string razorpayPaymentId,
        decimal? amountInRupees,
        CancellationToken cancellationToken = default) =>
        Task.FromResult($"rfnd_mock_{Guid.NewGuid():N}");
}

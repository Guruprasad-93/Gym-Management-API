namespace Gym.Application.Interfaces;

public interface IRazorpayGateway
{
    Task<string> CreateOrderAsync(
        decimal amountInRupees,
        string currency,
        string receipt,
        IReadOnlyDictionary<string, string>? notes,
        CancellationToken cancellationToken = default);

    bool VerifyPaymentSignature(string orderId, string paymentId, string signature);

    Task<string> RefundPaymentAsync(
        string razorpayPaymentId,
        decimal? amountInRupees,
        CancellationToken cancellationToken = default);
}

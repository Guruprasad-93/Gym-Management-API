using Gym.Application.DTOs.Payments;

namespace Gym.Application.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentResponseDto> CreateAsync(Guid gymId, CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponseDto>> GetHistoryAsync(Guid? gymId, string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponseDto>> GetByMemberAsync(int memberId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> GenerateInvoiceAsync(int paymentId, Guid gymId, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetInvoiceByIdAsync(int invoiceId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<RevenueDashboardDto> GetRevenueDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyRevenueDto>> GetMonthlyRevenueAsync(Guid? gymId, int months, CancellationToken cancellationToken = default);
    Task<PaymentResponseDto> CreateRazorpayOrderAsync(Guid gymId, int memberId, int membershipId, string razorpayOrderId, decimal amount, string? notes, CancellationToken cancellationToken = default);
    Task<PaymentResponseDto?> GetByRazorpayOrderIdAsync(string razorpayOrderId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<(PaymentResponseDto Payment, int? NewMembershipId)> ConfirmRazorpayPaymentAsync(Guid gymId, string razorpayOrderId, string razorpayPaymentId, bool renewMembership, CancellationToken cancellationToken = default);
    Task FailRazorpayPaymentAsync(Guid gymId, string razorpayOrderId, string? failureReason, CancellationToken cancellationToken = default);
    Task<PaymentResponseDto> RefundPaymentAsync(int paymentId, Guid gymId, string? refundReference, string? notes, CancellationToken cancellationToken = default);
    Task<RazorpayCheckoutContextDto?> GetMemberPayableMembershipAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default);
}

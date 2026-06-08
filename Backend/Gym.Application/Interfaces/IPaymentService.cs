using Gym.Application.DTOs.Payments;

namespace Gym.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponseDto>> GetHistoryAsync(string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponseDto>> GetByMemberAsync(int memberId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> GenerateInvoiceAsync(int paymentId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<RevenueDashboardDto> GetRevenueDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months, CancellationToken cancellationToken = default);
    Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(CreateRazorpayOrderDto dto, CancellationToken cancellationToken = default);
    Task<PaymentResponseDto> VerifyRazorpayPaymentAsync(VerifyRazorpayPaymentDto dto, CancellationToken cancellationToken = default);
    Task<RazorpayCheckoutContextDto?> GetCheckoutContextAsync(CancellationToken cancellationToken = default);
    Task<RefundPaymentResponseDto> RefundPaymentAsync(int paymentId, RefundPaymentDto dto, CancellationToken cancellationToken = default);
}

using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.VerifyRazorpayPayment;

public class VerifyRazorpayPaymentCommandHandler : IRequestHandler<VerifyRazorpayPaymentCommand, PaymentResponseDto>
{
    private readonly IPaymentService _paymentService;

    public VerifyRazorpayPaymentCommandHandler(IPaymentService paymentService) => _paymentService = paymentService;

    public Task<PaymentResponseDto> Handle(VerifyRazorpayPaymentCommand request, CancellationToken cancellationToken) =>
        _paymentService.VerifyRazorpayPaymentAsync(request.Dto, cancellationToken);
}

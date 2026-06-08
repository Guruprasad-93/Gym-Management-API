using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.RefundPayment;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, RefundPaymentResponseDto>
{
    private readonly IPaymentService _paymentService;

    public RefundPaymentCommandHandler(IPaymentService paymentService) => _paymentService = paymentService;

    public Task<RefundPaymentResponseDto> Handle(RefundPaymentCommand request, CancellationToken cancellationToken) =>
        _paymentService.RefundPaymentAsync(request.PaymentId, request.Dto, cancellationToken);
}

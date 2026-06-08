using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentResponseDto>
{
    private readonly IPaymentService _paymentService;

    public CreatePaymentCommandHandler(IPaymentService paymentService) =>
        _paymentService = paymentService;

    public Task<PaymentResponseDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken) =>
        _paymentService.CreateAsync(request.Dto, cancellationToken);
}

using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.CreateRazorpayOrder;

public class CreateRazorpayOrderCommandHandler : IRequestHandler<CreateRazorpayOrderCommand, RazorpayOrderResponseDto>
{
    private readonly IPaymentService _paymentService;

    public CreateRazorpayOrderCommandHandler(IPaymentService paymentService) => _paymentService = paymentService;

    public Task<RazorpayOrderResponseDto> Handle(CreateRazorpayOrderCommand request, CancellationToken cancellationToken) =>
        _paymentService.CreateRazorpayOrderAsync(request.Dto, cancellationToken);
}

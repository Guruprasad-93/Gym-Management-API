using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetRazorpayCheckoutContext;

public class GetRazorpayCheckoutContextQueryHandler : IRequestHandler<GetRazorpayCheckoutContextQuery, RazorpayCheckoutContextDto?>
{
    private readonly IPaymentService _paymentService;

    public GetRazorpayCheckoutContextQueryHandler(IPaymentService paymentService) => _paymentService = paymentService;

    public Task<RazorpayCheckoutContextDto?> Handle(GetRazorpayCheckoutContextQuery request, CancellationToken cancellationToken) =>
        _paymentService.GetCheckoutContextAsync(cancellationToken);
}

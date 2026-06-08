using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetPayments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, IReadOnlyList<PaymentResponseDto>>
{
    private readonly IPaymentService _paymentService;

    public GetPaymentsQueryHandler(IPaymentService paymentService) =>
        _paymentService = paymentService;

    public Task<IReadOnlyList<PaymentResponseDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken) =>
        _paymentService.GetHistoryAsync(request.Search, cancellationToken);
}

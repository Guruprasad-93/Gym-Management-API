using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetPaymentsByMember;

public class GetPaymentsByMemberQueryHandler : IRequestHandler<GetPaymentsByMemberQuery, IReadOnlyList<PaymentResponseDto>>
{
    private readonly IPaymentService _paymentService;

    public GetPaymentsByMemberQueryHandler(IPaymentService paymentService) =>
        _paymentService = paymentService;

    public Task<IReadOnlyList<PaymentResponseDto>> Handle(GetPaymentsByMemberQuery request, CancellationToken cancellationToken) =>
        _paymentService.GetByMemberAsync(request.MemberId, cancellationToken);
}

using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetMonthlyRevenue;

public class GetMonthlyRevenueQueryHandler : IRequestHandler<GetMonthlyRevenueQuery, IReadOnlyList<MonthlyRevenueDto>>
{
    private readonly IPaymentService _paymentService;

    public GetMonthlyRevenueQueryHandler(IPaymentService paymentService) =>
        _paymentService = paymentService;

    public Task<IReadOnlyList<MonthlyRevenueDto>> Handle(GetMonthlyRevenueQuery request, CancellationToken cancellationToken) =>
        _paymentService.GetMonthlyRevenueAsync(request.Months, cancellationToken);
}

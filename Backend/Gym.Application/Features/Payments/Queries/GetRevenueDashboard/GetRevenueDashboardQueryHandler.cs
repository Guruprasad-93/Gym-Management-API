using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetRevenueDashboard;

public class GetRevenueDashboardQueryHandler : IRequestHandler<GetRevenueDashboardQuery, RevenueDashboardDto>
{
    private readonly IPaymentService _paymentService;

    public GetRevenueDashboardQueryHandler(IPaymentService paymentService) =>
        _paymentService = paymentService;

    public Task<RevenueDashboardDto> Handle(GetRevenueDashboardQuery request, CancellationToken cancellationToken) =>
        _paymentService.GetRevenueDashboardAsync(request.GymId, cancellationToken);
}

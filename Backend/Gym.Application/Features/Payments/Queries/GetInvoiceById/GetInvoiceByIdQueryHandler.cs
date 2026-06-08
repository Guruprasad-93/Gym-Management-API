using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IPaymentService _paymentService;

    public GetInvoiceByIdQueryHandler(IPaymentService paymentService) => _paymentService = paymentService;

    public Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken) =>
        _paymentService.GetInvoiceAsync(request.InvoiceId, cancellationToken);
}

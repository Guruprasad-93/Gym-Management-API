using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.GenerateInvoice;

public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
{
    private readonly IPaymentService _paymentService;

    public GenerateInvoiceCommandHandler(IPaymentService paymentService) =>
        _paymentService = paymentService;

    public Task<InvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken) =>
        _paymentService.GenerateInvoiceAsync(request.PaymentId, cancellationToken);
}

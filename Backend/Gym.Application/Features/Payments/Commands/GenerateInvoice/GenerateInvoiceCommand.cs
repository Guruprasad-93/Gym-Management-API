using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.GenerateInvoice;

public record GenerateInvoiceCommand(int PaymentId) : IRequest<InvoiceDto>;

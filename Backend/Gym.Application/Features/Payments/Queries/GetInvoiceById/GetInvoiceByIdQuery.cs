using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(int InvoiceId) : IRequest<InvoiceDto>;

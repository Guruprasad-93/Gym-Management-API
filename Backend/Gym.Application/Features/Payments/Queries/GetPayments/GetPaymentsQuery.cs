using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetPayments;

public record GetPaymentsQuery(string? Search) : IRequest<IReadOnlyList<PaymentResponseDto>>;

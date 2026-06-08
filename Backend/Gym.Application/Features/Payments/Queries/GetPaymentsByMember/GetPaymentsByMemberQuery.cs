using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetPaymentsByMember;

public record GetPaymentsByMemberQuery(int MemberId) : IRequest<IReadOnlyList<PaymentResponseDto>>;

using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.RefundPayment;

public record RefundPaymentCommand(int PaymentId, RefundPaymentDto Dto) : IRequest<RefundPaymentResponseDto>;

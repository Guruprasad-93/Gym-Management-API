using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.CreatePayment;

public record CreatePaymentCommand(CreatePaymentDto Dto) : IRequest<PaymentResponseDto>;

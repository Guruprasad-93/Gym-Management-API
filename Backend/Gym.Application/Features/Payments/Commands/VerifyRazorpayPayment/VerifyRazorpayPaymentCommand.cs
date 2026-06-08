using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.VerifyRazorpayPayment;

public record VerifyRazorpayPaymentCommand(VerifyRazorpayPaymentDto Dto) : IRequest<PaymentResponseDto>;

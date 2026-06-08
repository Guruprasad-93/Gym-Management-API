using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Commands.CreateRazorpayOrder;

public record CreateRazorpayOrderCommand(CreateRazorpayOrderDto Dto) : IRequest<RazorpayOrderResponseDto>;

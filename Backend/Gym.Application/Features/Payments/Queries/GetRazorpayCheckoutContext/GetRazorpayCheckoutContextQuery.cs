using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetRazorpayCheckoutContext;

public record GetRazorpayCheckoutContextQuery : IRequest<RazorpayCheckoutContextDto?>;

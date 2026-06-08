using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetRevenueDashboard;

public record GetRevenueDashboardQuery(Guid? GymId = null) : IRequest<RevenueDashboardDto>;

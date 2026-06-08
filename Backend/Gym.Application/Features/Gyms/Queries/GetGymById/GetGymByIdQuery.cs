using Gym.Application.DTOs.Gyms;
using MediatR;

namespace Gym.Application.Features.Gyms.Queries.GetGymById;

public record GetGymByIdQuery(Guid Id) : IRequest<GymDto>;

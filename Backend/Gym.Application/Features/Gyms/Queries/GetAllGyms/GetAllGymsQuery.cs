using Gym.Application.DTOs.Gyms;
using MediatR;

namespace Gym.Application.Features.Gyms.Queries.GetAllGyms;

public record GetAllGymsQuery : IRequest<IReadOnlyList<GymDto>>;

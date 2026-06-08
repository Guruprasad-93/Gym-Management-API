using Gym.Application.DTOs.GymAdmins;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Queries.GetGymAdminById;

public record GetGymAdminByIdQuery(Guid UserId) : IRequest<GymAdminDto>;

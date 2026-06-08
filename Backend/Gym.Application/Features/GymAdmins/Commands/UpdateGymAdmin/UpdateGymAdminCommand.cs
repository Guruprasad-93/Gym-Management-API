using Gym.Application.DTOs.GymAdmins;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.UpdateGymAdmin;

public record UpdateGymAdminCommand(Guid UserId, UpdateGymAdminDto Dto) : IRequest<GymAdminDto>;

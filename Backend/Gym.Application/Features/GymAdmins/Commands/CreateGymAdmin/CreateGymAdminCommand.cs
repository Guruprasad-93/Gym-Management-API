using Gym.Application.DTOs.GymAdmins;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.CreateGymAdmin;

public record CreateGymAdminCommand(CreateGymAdminDto Dto) : IRequest<CreateGymAdminResultDto>;

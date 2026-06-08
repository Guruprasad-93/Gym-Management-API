using Gym.Application.DTOs.GymAdmins;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.ResendTemporaryPassword;

public record ResendTemporaryPasswordCommand(Guid UserId) : IRequest<ResendTemporaryPasswordResultDto>;

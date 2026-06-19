using Gym.Application.DTOs.Auth;
using MediatR;

namespace Gym.Application.Features.Auth.Commands.Login;

public record LoginCommand(string LoginIdentifier, string Password, Guid? GymId = null) : IRequest<LoginResponseDto>;

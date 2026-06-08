using Gym.Application.DTOs.Gyms;
using MediatR;

namespace Gym.Application.Features.Gyms.Commands.CreateGym;

public record CreateGymCommand(CreateGymDto Dto) : IRequest<GymDto>;

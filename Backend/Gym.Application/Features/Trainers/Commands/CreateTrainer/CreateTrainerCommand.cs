using Gym.Application.DTOs.Trainers;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.CreateTrainer;

public record CreateTrainerCommand(CreateTrainerDto Dto) : IRequest<TrainerDto>;

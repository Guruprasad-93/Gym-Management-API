using Gym.Application.DTOs.Trainers;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.UpdateTrainer;

public record UpdateTrainerCommand(int TrainerId, UpdateTrainerDto Dto) : IRequest<TrainerDto>;

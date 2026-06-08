using MediatR;

namespace Gym.Application.Features.Trainers.Commands.DeleteTrainer;

public record DeleteTrainerCommand(int TrainerId) : IRequest<Unit>;

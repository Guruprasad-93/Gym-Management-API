using Gym.Application.DTOs.Trainers;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.AssignMembersToTrainer;

public record AssignMembersToTrainerCommand(int TrainerId, AssignMembersToTrainerDto Dto) : IRequest<Unit>;

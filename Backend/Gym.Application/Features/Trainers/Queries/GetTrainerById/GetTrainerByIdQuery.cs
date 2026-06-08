using Gym.Application.DTOs.Trainers;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerById;

public record GetTrainerByIdQuery(int TrainerId) : IRequest<TrainerDto>;

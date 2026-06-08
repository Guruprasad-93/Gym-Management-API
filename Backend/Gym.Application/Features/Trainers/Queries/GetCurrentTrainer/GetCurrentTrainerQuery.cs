using Gym.Application.DTOs.Trainers;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetCurrentTrainer;

public record GetCurrentTrainerQuery : IRequest<TrainerDto>;

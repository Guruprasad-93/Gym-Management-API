using Gym.Application.DTOs.Trainers;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerDashboard;

public record GetTrainerDashboardQuery(int TrainerId) : IRequest<TrainerDashboardDto>;

using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerDashboard;

public class GetTrainerDashboardQueryHandler : IRequestHandler<GetTrainerDashboardQuery, TrainerDashboardDto>
{
    private readonly ITrainerService _trainerService;

    public GetTrainerDashboardQueryHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<TrainerDashboardDto> Handle(GetTrainerDashboardQuery request, CancellationToken cancellationToken) =>
        _trainerService.GetDashboardAsync(request.TrainerId, cancellationToken);
}

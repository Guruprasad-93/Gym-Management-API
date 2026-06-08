using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetCurrentTrainer;

public class GetCurrentTrainerQueryHandler : IRequestHandler<GetCurrentTrainerQuery, TrainerDto>
{
    private readonly ITrainerService _trainerService;

    public GetCurrentTrainerQueryHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<TrainerDto> Handle(GetCurrentTrainerQuery request, CancellationToken cancellationToken) =>
        _trainerService.GetCurrentAsync(cancellationToken);
}

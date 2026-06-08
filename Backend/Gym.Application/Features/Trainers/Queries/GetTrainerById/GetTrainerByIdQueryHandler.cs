using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerById;

public class GetTrainerByIdQueryHandler : IRequestHandler<GetTrainerByIdQuery, TrainerDto>
{
    private readonly ITrainerService _trainerService;

    public GetTrainerByIdQueryHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<TrainerDto> Handle(GetTrainerByIdQuery request, CancellationToken cancellationToken) =>
        _trainerService.GetByIdAsync(request.TrainerId, cancellationToken);
}

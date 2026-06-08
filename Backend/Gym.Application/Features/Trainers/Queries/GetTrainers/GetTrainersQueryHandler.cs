using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainers;

public class GetTrainersQueryHandler : IRequestHandler<GetTrainersQuery, PagedResultDto<TrainerDto>>
{
    private readonly ITrainerService _trainerService;

    public GetTrainersQueryHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<PagedResultDto<TrainerDto>> Handle(GetTrainersQuery request, CancellationToken cancellationToken) =>
        _trainerService.GetPagedAsync(
            new GetTrainersQueryDto
            {
                GymId = request.GymId,
                Search = request.Search,
                IncludeInactive = request.IncludeInactive,
                Paging = request.Paging
            },
            cancellationToken);
}

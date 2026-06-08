using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Trainers;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainers;

public record GetTrainersQuery(
    Guid? GymId,
    string? Search,
    bool IncludeInactive,
    PagedRequestDto Paging) : IRequest<PagedResultDto<TrainerDto>>;

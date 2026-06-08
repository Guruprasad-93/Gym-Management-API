using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerMembers;

public class GetTrainerMembersQueryHandler : IRequestHandler<GetTrainerMembersQuery, IReadOnlyList<MemberDto>>
{
    private readonly ITrainerService _trainerService;

    public GetTrainerMembersQueryHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<IReadOnlyList<MemberDto>> Handle(GetTrainerMembersQuery request, CancellationToken cancellationToken) =>
        _trainerService.GetMembersAsync(request.TrainerId, request.Search, cancellationToken);
}

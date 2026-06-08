using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetUnassignedMembers;

public class GetUnassignedMembersQueryHandler : IRequestHandler<GetUnassignedMembersQuery, IReadOnlyList<MemberDto>>
{
    private readonly ITrainerService _trainerService;

    public GetUnassignedMembersQueryHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<IReadOnlyList<MemberDto>> Handle(GetUnassignedMembersQuery request, CancellationToken cancellationToken) =>
        _trainerService.GetUnassignedMembersAsync(request.TrainerId, request.Search, cancellationToken);
}

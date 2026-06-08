using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Memberships.Queries.GetMemberships;

public class GetMembershipsQueryHandler : IRequestHandler<GetMembershipsQuery, IReadOnlyList<MembershipResponseDto>>
{
    private readonly IMembershipService _membershipService;

    public GetMembershipsQueryHandler(IMembershipService membershipService) =>
        _membershipService = membershipService;

    public Task<IReadOnlyList<MembershipResponseDto>> Handle(GetMembershipsQuery request, CancellationToken cancellationToken) =>
        _membershipService.GetAllAsync(request.Search, request.IncludeInactive, cancellationToken);
}

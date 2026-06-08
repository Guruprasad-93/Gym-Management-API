using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Memberships.Queries.GetExpiredMemberships;

public class GetExpiredMembershipsQueryHandler : IRequestHandler<GetExpiredMembershipsQuery, IReadOnlyList<MembershipResponseDto>>
{
    private readonly IMembershipService _membershipService;

    public GetExpiredMembershipsQueryHandler(IMembershipService membershipService) =>
        _membershipService = membershipService;

    public Task<IReadOnlyList<MembershipResponseDto>> Handle(GetExpiredMembershipsQuery request, CancellationToken cancellationToken) =>
        _membershipService.GetExpiredAsync(cancellationToken);
}

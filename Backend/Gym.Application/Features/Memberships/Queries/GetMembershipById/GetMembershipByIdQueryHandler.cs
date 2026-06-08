using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Memberships.Queries.GetMembershipById;

public class GetMembershipByIdQueryHandler : IRequestHandler<GetMembershipByIdQuery, MembershipResponseDto>
{
    private readonly IMembershipService _membershipService;

    public GetMembershipByIdQueryHandler(IMembershipService membershipService) =>
        _membershipService = membershipService;

    public Task<MembershipResponseDto> Handle(GetMembershipByIdQuery request, CancellationToken cancellationToken) =>
        _membershipService.GetByIdAsync(request.MembershipId, cancellationToken);
}

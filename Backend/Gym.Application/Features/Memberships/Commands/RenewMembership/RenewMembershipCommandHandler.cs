using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Memberships.Commands.RenewMembership;

public class RenewMembershipCommandHandler : IRequestHandler<RenewMembershipCommand, MembershipResponseDto>
{
    private readonly IMembershipService _membershipService;

    public RenewMembershipCommandHandler(IMembershipService membershipService) =>
        _membershipService = membershipService;

    public Task<MembershipResponseDto> Handle(RenewMembershipCommand request, CancellationToken cancellationToken) =>
        _membershipService.RenewAsync(request.MembershipId, request.Dto, cancellationToken);
}

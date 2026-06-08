using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Memberships.Commands.CancelMembership;

public class CancelMembershipCommandHandler : IRequestHandler<CancelMembershipCommand, Unit>
{
    private readonly IMembershipService _membershipService;

    public CancelMembershipCommandHandler(IMembershipService membershipService) =>
        _membershipService = membershipService;

    public async Task<Unit> Handle(CancelMembershipCommand request, CancellationToken cancellationToken)
    {
        await _membershipService.CancelAsync(request.MembershipId, request.Dto, cancellationToken);
        return Unit.Value;
    }
}

using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Commands.DeactivateMember;

public class DeactivateMemberCommandHandler : IRequestHandler<DeactivateMemberCommand, Unit>
{
    private readonly IMemberService _memberService;

    public DeactivateMemberCommandHandler(IMemberService memberService) => _memberService = memberService;

    public async Task<Unit> Handle(DeactivateMemberCommand request, CancellationToken cancellationToken)
    {
        await _memberService.DeactivateAsync(request.MemberId, cancellationToken);
        return Unit.Value;
    }
}

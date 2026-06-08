using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Commands.ActivateMember;

public class ActivateMemberCommandHandler : IRequestHandler<ActivateMemberCommand, Unit>
{
    private readonly IMemberService _memberService;

    public ActivateMemberCommandHandler(IMemberService memberService) => _memberService = memberService;

    public async Task<Unit> Handle(ActivateMemberCommand request, CancellationToken cancellationToken)
    {
        await _memberService.ActivateAsync(request.MemberId, cancellationToken);
        return Unit.Value;
    }
}

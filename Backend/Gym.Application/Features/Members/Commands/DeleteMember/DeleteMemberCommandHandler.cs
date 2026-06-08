using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Unit>
{
    private readonly IMemberService _memberService;

    public DeleteMemberCommandHandler(IMemberService memberService) => _memberService = memberService;

    public async Task<Unit> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        await _memberService.DeleteAsync(request.MemberId, cancellationToken);
        return Unit.Value;
    }
}

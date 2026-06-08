using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Commands.RemoveTrainerAssignment;

public class RemoveTrainerAssignmentCommandHandler : IRequestHandler<RemoveTrainerAssignmentCommand, Unit>
{
    private readonly IMemberService _memberService;

    public RemoveTrainerAssignmentCommandHandler(IMemberService memberService) => _memberService = memberService;

    public async Task<Unit> Handle(RemoveTrainerAssignmentCommand request, CancellationToken cancellationToken)
    {
        await _memberService.RemoveTrainerAssignmentAsync(request.MemberId, cancellationToken);
        return Unit.Value;
    }
}

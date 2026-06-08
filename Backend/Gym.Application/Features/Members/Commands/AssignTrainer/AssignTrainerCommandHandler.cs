using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Commands.AssignTrainer;

public class AssignTrainerCommandHandler : IRequestHandler<AssignTrainerCommand, Unit>
{
    private readonly IMemberService _memberService;

    public AssignTrainerCommandHandler(IMemberService memberService) => _memberService = memberService;

    public async Task<Unit> Handle(AssignTrainerCommand request, CancellationToken cancellationToken)
    {
        await _memberService.AssignTrainerAsync(request.MemberId, request.Dto, cancellationToken);
        return Unit.Value;
    }
}

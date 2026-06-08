using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.RemoveMemberAssignment;

public class RemoveMemberAssignmentCommandHandler : IRequestHandler<RemoveMemberAssignmentCommand, Unit>
{
    private readonly ITrainerService _trainerService;

    public RemoveMemberAssignmentCommandHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public async Task<Unit> Handle(RemoveMemberAssignmentCommand request, CancellationToken cancellationToken)
    {
        await _trainerService.RemoveMemberAssignmentAsync(request.MemberId, cancellationToken);
        return Unit.Value;
    }
}

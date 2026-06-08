using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.AssignMembersToTrainer;

public class AssignMembersToTrainerCommandHandler : IRequestHandler<AssignMembersToTrainerCommand, Unit>
{
    private readonly ITrainerService _trainerService;

    public AssignMembersToTrainerCommandHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public async Task<Unit> Handle(AssignMembersToTrainerCommand request, CancellationToken cancellationToken)
    {
        await _trainerService.AssignMembersAsync(request.TrainerId, request.Dto, cancellationToken);
        return Unit.Value;
    }
}

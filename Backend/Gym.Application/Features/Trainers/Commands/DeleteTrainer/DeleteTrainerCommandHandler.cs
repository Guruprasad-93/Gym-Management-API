using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.DeleteTrainer;

public class DeleteTrainerCommandHandler : IRequestHandler<DeleteTrainerCommand, Unit>
{
    private readonly ITrainerService _trainerService;

    public DeleteTrainerCommandHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public async Task<Unit> Handle(DeleteTrainerCommand request, CancellationToken cancellationToken)
    {
        await _trainerService.DeleteAsync(request.TrainerId, cancellationToken);
        return Unit.Value;
    }
}

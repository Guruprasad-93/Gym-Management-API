using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.UpdateTrainer;

public class UpdateTrainerCommandHandler : IRequestHandler<UpdateTrainerCommand, TrainerDto>
{
    private readonly ITrainerService _trainerService;

    public UpdateTrainerCommandHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<TrainerDto> Handle(UpdateTrainerCommand request, CancellationToken cancellationToken) =>
        _trainerService.UpdateAsync(request.TrainerId, request.Dto, cancellationToken);
}

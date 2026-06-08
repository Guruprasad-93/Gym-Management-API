using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Trainers.Commands.CreateTrainer;

public class CreateTrainerCommandHandler : IRequestHandler<CreateTrainerCommand, TrainerDto>
{
    private readonly ITrainerService _trainerService;

    public CreateTrainerCommandHandler(ITrainerService trainerService) =>
        _trainerService = trainerService;

    public Task<TrainerDto> Handle(CreateTrainerCommand request, CancellationToken cancellationToken) =>
        _trainerService.CreateAsync(request.Dto, cancellationToken);
}

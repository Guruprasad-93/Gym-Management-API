using Gym.Application.DTOs.Gyms;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Gyms.Commands.CreateGym;

public class CreateGymCommandHandler : IRequestHandler<CreateGymCommand, GymDto>
{
    private readonly IGymService _gymService;

    public CreateGymCommandHandler(IGymService gymService) => _gymService = gymService;

    public Task<GymDto> Handle(CreateGymCommand request, CancellationToken cancellationToken) =>
        _gymService.CreateAsync(request.Dto, cancellationToken);
}

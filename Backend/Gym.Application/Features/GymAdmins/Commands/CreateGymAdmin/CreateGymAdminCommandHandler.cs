using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.CreateGymAdmin;

public class CreateGymAdminCommandHandler : IRequestHandler<CreateGymAdminCommand, CreateGymAdminResultDto>
{
    private readonly IGymAdminService _gymAdminService;

    public CreateGymAdminCommandHandler(IGymAdminService gymAdminService) =>
        _gymAdminService = gymAdminService;

    public Task<CreateGymAdminResultDto> Handle(CreateGymAdminCommand request, CancellationToken cancellationToken) =>
        _gymAdminService.CreateAsync(request.Dto, cancellationToken);
}

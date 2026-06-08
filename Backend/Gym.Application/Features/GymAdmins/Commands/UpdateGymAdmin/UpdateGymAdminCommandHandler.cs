using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.UpdateGymAdmin;

public class UpdateGymAdminCommandHandler : IRequestHandler<UpdateGymAdminCommand, GymAdminDto>
{
    private readonly IGymAdminService _gymAdminService;

    public UpdateGymAdminCommandHandler(IGymAdminService gymAdminService) =>
        _gymAdminService = gymAdminService;

    public Task<GymAdminDto> Handle(UpdateGymAdminCommand request, CancellationToken cancellationToken) =>
        _gymAdminService.UpdateAsync(request.UserId, request.Dto, cancellationToken);
}

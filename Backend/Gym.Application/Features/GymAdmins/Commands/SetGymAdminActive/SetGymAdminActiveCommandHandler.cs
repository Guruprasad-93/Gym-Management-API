using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.SetGymAdminActive;

public class SetGymAdminActiveCommandHandler : IRequestHandler<SetGymAdminActiveCommand>
{
    private readonly IGymAdminService _gymAdminService;

    public SetGymAdminActiveCommandHandler(IGymAdminService gymAdminService) =>
        _gymAdminService = gymAdminService;

    public Task Handle(SetGymAdminActiveCommand request, CancellationToken cancellationToken) =>
        _gymAdminService.SetActiveAsync(request.UserId, request.IsActive, cancellationToken);
}

using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.ResendTemporaryPassword;

public class ResendTemporaryPasswordCommandHandler
    : IRequestHandler<ResendTemporaryPasswordCommand, ResendTemporaryPasswordResultDto>
{
    private readonly IGymAdminService _gymAdminService;

    public ResendTemporaryPasswordCommandHandler(IGymAdminService gymAdminService) =>
        _gymAdminService = gymAdminService;

    public Task<ResendTemporaryPasswordResultDto> Handle(
        ResendTemporaryPasswordCommand request,
        CancellationToken cancellationToken) =>
        _gymAdminService.ResendTemporaryPasswordAsync(request.UserId, cancellationToken);
}

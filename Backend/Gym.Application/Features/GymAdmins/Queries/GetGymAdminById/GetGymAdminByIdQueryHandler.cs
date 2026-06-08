using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Queries.GetGymAdminById;

public class GetGymAdminByIdQueryHandler : IRequestHandler<GetGymAdminByIdQuery, GymAdminDto>
{
    private readonly IGymAdminService _gymAdminService;

    public GetGymAdminByIdQueryHandler(IGymAdminService gymAdminService) =>
        _gymAdminService = gymAdminService;

    public Task<GymAdminDto> Handle(GetGymAdminByIdQuery request, CancellationToken cancellationToken) =>
        _gymAdminService.GetByIdAsync(request.UserId, cancellationToken);
}

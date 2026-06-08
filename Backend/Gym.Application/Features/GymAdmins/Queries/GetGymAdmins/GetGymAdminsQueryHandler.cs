using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Queries.GetGymAdmins;

public class GetGymAdminsQueryHandler : IRequestHandler<GetGymAdminsQuery, PagedResultDto<GymAdminDto>>
{
    private readonly IGymAdminService _gymAdminService;

    public GetGymAdminsQueryHandler(IGymAdminService gymAdminService) =>
        _gymAdminService = gymAdminService;

    public Task<PagedResultDto<GymAdminDto>> Handle(GetGymAdminsQuery request, CancellationToken cancellationToken) =>
        _gymAdminService.GetAllAsync(request.GymId, request.Paging, cancellationToken);
}

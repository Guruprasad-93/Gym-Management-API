using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.GymAdmins;
using MediatR;

namespace Gym.Application.Features.GymAdmins.Queries.GetGymAdmins;

public record GetGymAdminsQuery(Guid? GymId, PagedRequestDto Paging) : IRequest<PagedResultDto<GymAdminDto>>;

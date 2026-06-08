using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetMembers;

public record GetMembersQuery(
    Guid? GymId,
    string? Search,
    bool IncludeInactive,
    PagedRequestDto Paging) : IRequest<PagedResultDto<MemberResponseDto>>;

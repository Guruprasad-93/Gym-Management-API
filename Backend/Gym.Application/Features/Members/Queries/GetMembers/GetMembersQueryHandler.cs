using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetMembers;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, PagedResultDto<MemberResponseDto>>
{
    private readonly IMemberService _memberService;

    public GetMembersQueryHandler(IMemberService memberService) => _memberService = memberService;

    public Task<PagedResultDto<MemberResponseDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken) =>
        _memberService.GetPagedAsync(
            new GetMembersQueryDto
            {
                GymId = request.GymId,
                Search = request.Search,
                IncludeInactive = request.IncludeInactive,
                Paging = request.Paging
            },
            cancellationToken);
}

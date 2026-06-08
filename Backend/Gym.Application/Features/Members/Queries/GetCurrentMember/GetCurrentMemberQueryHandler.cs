using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetCurrentMember;

public class GetCurrentMemberQueryHandler : IRequestHandler<GetCurrentMemberQuery, MemberResponseDto>
{
    private readonly IMemberService _memberService;

    public GetCurrentMemberQueryHandler(IMemberService memberService) =>
        _memberService = memberService;

    public Task<MemberResponseDto> Handle(GetCurrentMemberQuery request, CancellationToken cancellationToken) =>
        _memberService.GetCurrentAsync(cancellationToken);
}

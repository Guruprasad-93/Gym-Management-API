using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberResponseDto>
{
    private readonly IMemberService _memberService;

    public GetMemberByIdQueryHandler(IMemberService memberService) => _memberService = memberService;

    public Task<MemberResponseDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken) =>
        _memberService.GetByIdAsync(request.MemberId, cancellationToken);
}

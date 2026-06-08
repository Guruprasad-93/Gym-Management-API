using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetMemberDetails;

public class GetMemberDetailsQueryHandler : IRequestHandler<GetMemberDetailsQuery, MemberDetailsDto>
{
    private readonly IMemberService _memberService;

    public GetMemberDetailsQueryHandler(IMemberService memberService) => _memberService = memberService;

    public Task<MemberDetailsDto> Handle(GetMemberDetailsQuery request, CancellationToken cancellationToken) =>
        _memberService.GetDetailsAsync(request.MemberId, cancellationToken);
}

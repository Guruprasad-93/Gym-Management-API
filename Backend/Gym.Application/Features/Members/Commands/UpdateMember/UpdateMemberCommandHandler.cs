using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, MemberResponseDto>
{
    private readonly IMemberService _memberService;

    public UpdateMemberCommandHandler(IMemberService memberService) => _memberService = memberService;

    public Task<MemberResponseDto> Handle(UpdateMemberCommand request, CancellationToken cancellationToken) =>
        _memberService.UpdateAsync(request.MemberId, request.Dto, cancellationToken);
}

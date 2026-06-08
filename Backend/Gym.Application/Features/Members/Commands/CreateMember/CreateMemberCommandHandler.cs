using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, MemberResponseDto>
{
    private readonly IMemberService _memberService;

    public CreateMemberCommandHandler(IMemberService memberService) => _memberService = memberService;

    public Task<MemberResponseDto> Handle(CreateMemberCommand request, CancellationToken cancellationToken) =>
        _memberService.CreateAsync(request.Dto, cancellationToken);
}

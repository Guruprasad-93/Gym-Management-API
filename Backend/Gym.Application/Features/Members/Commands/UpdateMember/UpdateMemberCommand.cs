using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Members.Commands.UpdateMember;

public record UpdateMemberCommand(int MemberId, UpdateMemberDto Dto) : IRequest<MemberResponseDto>;

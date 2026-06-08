using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Members.Commands.CreateMember;

public record CreateMemberCommand(CreateMemberDto Dto) : IRequest<MemberResponseDto>;

using MediatR;

namespace Gym.Application.Features.Members.Commands.DeleteMember;

public record DeleteMemberCommand(int MemberId) : IRequest<Unit>;

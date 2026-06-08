using MediatR;

namespace Gym.Application.Features.Members.Commands.DeactivateMember;

public record DeactivateMemberCommand(int MemberId) : IRequest<Unit>;

using MediatR;

namespace Gym.Application.Features.Members.Commands.ActivateMember;

public record ActivateMemberCommand(int MemberId) : IRequest<Unit>;

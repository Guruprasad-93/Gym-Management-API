using MediatR;

namespace Gym.Application.Features.Members.Commands.RemoveTrainerAssignment;

public record RemoveTrainerAssignmentCommand(int MemberId) : IRequest<Unit>;

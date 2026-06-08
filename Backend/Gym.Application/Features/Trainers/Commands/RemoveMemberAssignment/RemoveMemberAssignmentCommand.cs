using MediatR;

namespace Gym.Application.Features.Trainers.Commands.RemoveMemberAssignment;

public record RemoveMemberAssignmentCommand(int MemberId) : IRequest<Unit>;

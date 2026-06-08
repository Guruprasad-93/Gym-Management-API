using FluentValidation;

namespace Gym.Application.Features.Trainers.Commands.RemoveMemberAssignment;

public class RemoveMemberAssignmentCommandValidator : AbstractValidator<RemoveMemberAssignmentCommand>
{
    public RemoveMemberAssignmentCommandValidator() =>
        RuleFor(x => x.MemberId).GreaterThan(0);
}

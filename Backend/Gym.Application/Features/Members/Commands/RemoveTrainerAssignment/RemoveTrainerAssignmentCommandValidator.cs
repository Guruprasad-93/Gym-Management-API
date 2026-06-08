using FluentValidation;

namespace Gym.Application.Features.Members.Commands.RemoveTrainerAssignment;

public class RemoveTrainerAssignmentCommandValidator : AbstractValidator<RemoveTrainerAssignmentCommand>
{
    public RemoveTrainerAssignmentCommandValidator() => RuleFor(x => x.MemberId).GreaterThan(0);
}

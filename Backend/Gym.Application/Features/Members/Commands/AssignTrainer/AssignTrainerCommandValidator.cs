using FluentValidation;

namespace Gym.Application.Features.Members.Commands.AssignTrainer;

public class AssignTrainerCommandValidator : AbstractValidator<AssignTrainerCommand>
{
    public AssignTrainerCommandValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.Dto).SetValidator(new Validators.AssignTrainerToMemberDtoValidator());
    }
}

using FluentValidation;

namespace Gym.Application.Features.Trainers.Commands.AssignMembersToTrainer;

public class AssignMembersToTrainerCommandValidator : AbstractValidator<AssignMembersToTrainerCommand>
{
    public AssignMembersToTrainerCommandValidator()
    {
        RuleFor(x => x.TrainerId).GreaterThan(0);
        RuleFor(x => x.Dto).SetValidator(new Validators.AssignMembersToTrainerDtoValidator());
    }
}

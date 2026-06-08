using FluentValidation;

namespace Gym.Application.Features.Trainers.Commands.CreateTrainer;

public class CreateTrainerCommandValidator : AbstractValidator<CreateTrainerCommand>
{
    public CreateTrainerCommandValidator() =>
        RuleFor(x => x.Dto).SetValidator(new Validators.CreateTrainerDtoValidator());
}

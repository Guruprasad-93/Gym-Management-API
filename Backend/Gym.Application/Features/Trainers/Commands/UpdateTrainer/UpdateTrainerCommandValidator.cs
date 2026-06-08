using FluentValidation;

namespace Gym.Application.Features.Trainers.Commands.UpdateTrainer;

public class UpdateTrainerCommandValidator : AbstractValidator<UpdateTrainerCommand>
{
    public UpdateTrainerCommandValidator()
    {
        RuleFor(x => x.TrainerId).GreaterThan(0);
        RuleFor(x => x.Dto).SetValidator(new Validators.UpdateTrainerDtoValidator());
    }
}

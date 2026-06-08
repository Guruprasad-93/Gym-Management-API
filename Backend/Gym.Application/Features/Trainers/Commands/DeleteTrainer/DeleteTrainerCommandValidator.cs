using FluentValidation;

namespace Gym.Application.Features.Trainers.Commands.DeleteTrainer;

public class DeleteTrainerCommandValidator : AbstractValidator<DeleteTrainerCommand>
{
    public DeleteTrainerCommandValidator() =>
        RuleFor(x => x.TrainerId).GreaterThan(0);
}

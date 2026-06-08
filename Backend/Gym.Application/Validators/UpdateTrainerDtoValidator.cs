using FluentValidation;
using Gym.Application.DTOs.Trainers;

namespace Gym.Application.Validators;

public class UpdateTrainerDtoValidator : AbstractValidator<UpdateTrainerDto>
{
    public UpdateTrainerDtoValidator()
    {
        RuleFor(x => x.Specialization).MaximumLength(200);
        RuleFor(x => x.Bio).MaximumLength(1000);
    }
}

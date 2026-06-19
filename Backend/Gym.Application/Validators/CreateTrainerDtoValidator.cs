using FluentValidation;
using Gym.Application.DTOs.Trainers;

namespace Gym.Application.Validators;

public class CreateTrainerDtoValidator : AbstractValidator<CreateTrainerDto>
{
    public CreateTrainerDtoValidator()
    {
        When(x => x.UserId is null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LoginIdentifier!).ValidLoginIdentifier();
            RuleFor(x => x.Email)
                .EmailAddress().MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.Password!).NotEmpty().MinimumLength(8).MaximumLength(100);
        });

        RuleFor(x => x.Specialization).MaximumLength(200);
        RuleFor(x => x.Bio).MaximumLength(1000);
    }
}

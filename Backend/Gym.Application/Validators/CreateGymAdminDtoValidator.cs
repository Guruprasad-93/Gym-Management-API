using FluentValidation;
using Gym.Application.DTOs.GymAdmins;

namespace Gym.Application.Validators;

public class CreateGymAdminDtoValidator : AbstractValidator<CreateGymAdminDto>
{
    public CreateGymAdminDtoValidator()
    {
        RuleFor(x => x.GymId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
        RuleFor(x => x.Email)
            .EmailAddress().MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Password)
            .MinimumLength(8).MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Password));
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Password) || x.GenerateTemporaryPassword)
            .WithMessage("Either provide a password or enable temporary password generation.");
    }
}

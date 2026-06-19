using FluentValidation;
using Gym.Application.DTOs.Saas;
using Gym.Application.Validation;

namespace Gym.Application.Validators;

public class RegisterGymDtoValidator : AbstractValidator<RegisterGymDto>
{
    public RegisterGymDtoValidator()
    {
        RuleFor(x => x.GymName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Mobile).RequiredPhoneNumber();
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
        RuleFor(x => x.Email)
            .EmailAddress().MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Password).MinimumLength(6).When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}

using FluentValidation;
using Gym.Application.DTOs.Saas;

namespace Gym.Application.Validators;

public class RegisterGymDtoValidator : AbstractValidator<RegisterGymDto>
{
    public RegisterGymDtoValidator()
    {
        RuleFor(x => x.GymName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Mobile).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Password).MinimumLength(6).When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}

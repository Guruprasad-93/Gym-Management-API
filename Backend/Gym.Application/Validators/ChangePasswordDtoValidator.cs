using FluentValidation;
using Gym.Application.DTOs.Auth;

namespace Gym.Application.Validators;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Must((dto, np) => !string.Equals(dto.CurrentPassword, np, StringComparison.Ordinal))
            .WithMessage("New password must be different from the current password.");
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword)
            .WithMessage("Confirm Password does not match New Password.");
    }
}

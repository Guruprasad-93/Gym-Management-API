using FluentValidation;
using Gym.Application.DTOs.Auth;

namespace Gym.Application.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
        RuleFor(x => x.Token).NotEmpty().MaximumLength(512);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword)
            .WithMessage("Confirm Password does not match New Password.");
    }
}

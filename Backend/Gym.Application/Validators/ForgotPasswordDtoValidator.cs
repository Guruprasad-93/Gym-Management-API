using FluentValidation;
using Gym.Application.DTOs.Auth;

namespace Gym.Application.Validators;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
    }
}

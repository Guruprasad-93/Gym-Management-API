using FluentValidation;
using Gym.Application.DTOs.Auth;

namespace Gym.Application.Validators;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

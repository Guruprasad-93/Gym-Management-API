using FluentValidation;
using Gym.Application.Validators;

namespace Gym.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.LoginIdentifier).ValidLoginIdentifier();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

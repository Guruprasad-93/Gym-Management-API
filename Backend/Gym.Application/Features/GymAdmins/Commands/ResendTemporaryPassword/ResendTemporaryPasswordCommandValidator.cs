using FluentValidation;

namespace Gym.Application.Features.GymAdmins.Commands.ResendTemporaryPassword;

public class ResendTemporaryPasswordCommandValidator : AbstractValidator<ResendTemporaryPasswordCommand>
{
    public ResendTemporaryPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

using FluentValidation;

namespace Gym.Application.Features.GymAdmins.Commands.SetGymAdminActive;

public class SetGymAdminActiveCommandValidator : AbstractValidator<SetGymAdminActiveCommand>
{
    public SetGymAdminActiveCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

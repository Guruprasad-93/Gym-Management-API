using FluentValidation;

namespace Gym.Application.Features.GymAdmins.Commands.CreateGymAdmin;

public class CreateGymAdminCommandValidator : AbstractValidator<CreateGymAdminCommand>
{
    public CreateGymAdminCommandValidator()
    {
        RuleFor(x => x.Dto).SetValidator(new Validators.CreateGymAdminDtoValidator());
    }
}

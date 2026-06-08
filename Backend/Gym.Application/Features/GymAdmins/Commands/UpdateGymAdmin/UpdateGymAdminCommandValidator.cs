using FluentValidation;

namespace Gym.Application.Features.GymAdmins.Commands.UpdateGymAdmin;

public class UpdateGymAdminCommandValidator : AbstractValidator<UpdateGymAdminCommand>
{
    public UpdateGymAdminCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).SetValidator(new Validators.UpdateGymAdminDtoValidator());
    }
}

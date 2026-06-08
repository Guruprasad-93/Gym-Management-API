using FluentValidation;

namespace Gym.Application.Features.GymAdmins.Queries.GetGymAdminById;

public class GetGymAdminByIdQueryValidator : AbstractValidator<GetGymAdminByIdQuery>
{
    public GetGymAdminByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

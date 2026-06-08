using FluentValidation;

namespace Gym.Application.Features.GymAdmins.Queries.GetGymAdmins;

public class GetGymAdminsQueryValidator : AbstractValidator<GetGymAdminsQuery>
{
    public GetGymAdminsQueryValidator()
    {
        RuleFor(x => x.Paging).SetValidator(new Validators.PagedRequestDtoValidator());
    }
}

using FluentValidation;
using Gym.Application.DTOs.Trainers;

namespace Gym.Application.Validators;

public class AssignMembersToTrainerDtoValidator : AbstractValidator<AssignMembersToTrainerDto>
{
    public AssignMembersToTrainerDtoValidator()
    {
        RuleFor(x => x.MemberIds).NotEmpty();
        RuleForEach(x => x.MemberIds).GreaterThan(0);
    }
}

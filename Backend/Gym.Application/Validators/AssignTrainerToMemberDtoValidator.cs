using FluentValidation;
using Gym.Application.DTOs.Members;

namespace Gym.Application.Validators;

public class AssignTrainerToMemberDtoValidator : AbstractValidator<AssignTrainerToMemberDto>
{
    public AssignTrainerToMemberDtoValidator() =>
        RuleFor(x => x.TrainerId).GreaterThan(0);
}

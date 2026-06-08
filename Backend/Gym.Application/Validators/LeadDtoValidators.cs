using FluentValidation;
using Gym.Application.Constants;
using Gym.Application.DTOs.Leads;

namespace Gym.Application.Validators;

public class CreateLeadDtoValidator : AbstractValidator<CreateLeadDto>
{
    public CreateLeadDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(256);
        RuleFor(x => x.LeadSource).NotEmpty().Must(s => LeadSources.All.Contains(s)).WithMessage("Invalid lead source.");
        RuleFor(x => x.Gender).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Age).InclusiveBetween(1, 120).When(x => x.Age.HasValue);
        RuleFor(x => x.Status).Must(s => s is null || LeadStatuses.All.Contains(s)).WithMessage("Invalid status.");
    }
}

public class UpdateLeadDtoValidator : AbstractValidator<UpdateLeadDto>
{
    public UpdateLeadDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(256);
        RuleFor(x => x.LeadSource).NotEmpty().Must(s => LeadSources.All.Contains(s)).WithMessage("Invalid lead source.");
        RuleFor(x => x.Status).NotEmpty().Must(s => LeadStatuses.All.Contains(s)).WithMessage("Invalid status.");
        RuleFor(x => x.Gender).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Age).InclusiveBetween(1, 120).When(x => x.Age.HasValue);
    }
}

public class ConvertLeadToMemberDtoValidator : AbstractValidator<ConvertLeadToMemberDto>
{
    public ConvertLeadToMemberDtoValidator()
    {
        RuleFor(x => x.LeadId).GreaterThan(0);
        RuleFor(x => x.MembershipPlanId).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(256);
        RuleFor(x => x.Password).MinimumLength(8).When(x => !string.IsNullOrWhiteSpace(x.Password)).MaximumLength(100);
    }
}

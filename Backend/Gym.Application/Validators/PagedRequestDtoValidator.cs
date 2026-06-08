using FluentValidation;
using Gym.Application.DTOs.Common;

namespace Gym.Application.Validators;

public class PagedRequestDtoValidator : AbstractValidator<PagedRequestDto>
{
    public PagedRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortColumn).MaximumLength(50);
        RuleFor(x => x.SortDirection)
            .Must(d => string.IsNullOrWhiteSpace(d) ||
                       d.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                       d.Equals("desc", StringComparison.OrdinalIgnoreCase));
    }
}

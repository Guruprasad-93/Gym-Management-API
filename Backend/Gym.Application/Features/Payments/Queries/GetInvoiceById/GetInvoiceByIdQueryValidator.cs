using FluentValidation;

namespace Gym.Application.Features.Payments.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator() => RuleFor(x => x.InvoiceId).GreaterThan(0);
}

using FluentValidation;

namespace Dashboard.Application.Widgets.Queries.GetWidgets;

public sealed class GetWidgetsQueryValidator : AbstractValidator<GetWidgetsQuery>
{
    public GetWidgetsQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
        RuleFor(x => x.After).GreaterThanOrEqualTo(0).When(x => x.After.HasValue);
    }
}

using Dashboard.Domain.Enums;
using FluentValidation;

namespace Dashboard.Application.Widgets.Commands.CreateWidget;

public sealed class CreateWidgetCommandValidator : AbstractValidator<CreateWidgetCommand>
{
    public CreateWidgetCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(type => WidgetTypeParser.TryParse(type, out _))
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<WidgetType>())}.");
    }
}

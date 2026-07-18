using Dashboard.Domain.Enums;
using FluentValidation;

namespace Dashboard.Application.Widgets.Commands.CreateWidget;

public sealed class CreateWidgetCommandValidator : AbstractValidator<CreateWidgetCommand>
{
    public CreateWidgetCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(BeAValidWidgetType)
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<WidgetType>())}.");
    }

    private static bool BeAValidWidgetType(string type) =>
        Enum.TryParse<WidgetType>(type, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed);
}

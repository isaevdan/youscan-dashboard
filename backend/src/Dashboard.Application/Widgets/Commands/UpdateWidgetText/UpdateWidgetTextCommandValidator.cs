using FluentValidation;

namespace Dashboard.Application.Widgets.Commands.UpdateWidgetText;

public sealed class UpdateWidgetTextCommandValidator : AbstractValidator<UpdateWidgetTextCommand>
{
    public const int MaxTextLength = 5000;

    public UpdateWidgetTextCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        // Empty string is allowed on purpose (clearing a text widget) — only null is rejected.
        RuleFor(x => x.Text)
            .NotNull().WithMessage("'Text' must not be null.")
            .MaximumLength(MaxTextLength);
    }
}

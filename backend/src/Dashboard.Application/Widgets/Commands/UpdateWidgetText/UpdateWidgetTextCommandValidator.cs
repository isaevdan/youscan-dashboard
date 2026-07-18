using FluentValidation;

namespace Dashboard.Application.Widgets.Commands.UpdateWidgetText;

public sealed class UpdateWidgetTextCommandValidator : AbstractValidator<UpdateWidgetTextCommand>
{
    public UpdateWidgetTextCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Text).NotNull();
    }
}

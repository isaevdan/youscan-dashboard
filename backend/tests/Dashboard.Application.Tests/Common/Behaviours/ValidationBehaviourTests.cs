using Dashboard.Application.Common.Behaviours;
using Dashboard.Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Xunit;

namespace Dashboard.Application.Tests.Common.Behaviours;

public class ValidationBehaviourTests
{
    public record SampleRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behaviour = new ValidationBehaviour<SampleRequest, string>(Array.Empty<IValidator<SampleRequest>>());
        var nextCalled = false;

        var result = await behaviour.Handle(new SampleRequest("x"), _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ValidatorPasses_CallsNext()
    {
        var validator = Substitute.For<IValidator<SampleRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behaviour = new ValidationBehaviour<SampleRequest, string>(new[] { validator });

        var result = await behaviour.Handle(new SampleRequest("x"), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ValidatorFails_ThrowsValidationException()
    {
        var failure = new ValidationFailure("Value", "Value is invalid");
        var validator = Substitute.For<IValidator<SampleRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { failure }));

        var behaviour = new ValidationBehaviour<SampleRequest, string>(new[] { validator });

        await Assert.ThrowsAsync<Dashboard.Application.Common.Exceptions.ValidationException>(
            () => behaviour.Handle(new SampleRequest("x"), _ => Task.FromResult("ok"), CancellationToken.None));
    }
}

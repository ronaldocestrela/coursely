using Application.Features.Auth.Commands.ForgotPassword;
using FluentAssertions;
using FluentValidation;

namespace UnitTests;

public sealed class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_fail_when_email_empty(string email)
    {
        var command = new ForgotPasswordCommand(email);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
    }

    [Fact]
    public async Task Should_fail_when_email_invalid_format()
    {
        var command = new ForgotPasswordCommand("not-valid");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ForgotPasswordCommand.Email));
    }

    [Fact]
    public async Task Should_pass_when_email_valid()
    {
        var command = new ForgotPasswordCommand("recover@example.com");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}

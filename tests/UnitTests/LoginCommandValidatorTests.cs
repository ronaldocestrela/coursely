using Application.Features.Auth.Commands.Login;
using FluentAssertions;

namespace UnitTests;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_fail_when_email_empty(string email)
    {
        var command = new LoginCommand(email, "any");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public async Task Should_fail_when_email_invalid()
    {
        var command = new LoginCommand("not-an-email", "secret");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public async Task Should_fail_when_password_empty()
    {
        var command = new LoginCommand("jane@example.com", "");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public async Task Should_pass_when_valid()
    {
        var command = new LoginCommand("jane@example.com", "Strong1!x");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}

using Application.Features.Auth.Commands.RegisterUser;
using FluentAssertions;
using FluentValidation;

namespace UnitTests;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_fail_when_name_empty(string name)
    {
        var command = new RegisterUserCommand(name, "a@b.com", "Strong1!a");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Name));
    }

    [Fact]
    public async Task Should_fail_when_email_invalid()
    {
        var command = new RegisterUserCommand("Jane", "not-an-email", "Strong1!a");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("short1!")] // too short
    [InlineData("noupper1!")] // no upper
    [InlineData("NOLOWER1!")] // no lower
    [InlineData("NoDigit!!")] // no digit
    [InlineData("NoSpecial1")] // no special
    public async Task Should_fail_when_password_weak(string password)
    {
        var command = new RegisterUserCommand("Jane", "j@b.com", password);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Fact]
    public async Task Should_pass_when_valid()
    {
        var command = new RegisterUserCommand("Jane", "jane@example.com", "Strong1!x");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}

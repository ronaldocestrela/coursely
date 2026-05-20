using Application.Features.Auth.Commands.ResetPassword;
using FluentAssertions;
using FluentValidation;

namespace UnitTests;

public sealed class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public async Task Should_fail_when_user_id_empty_guid()
    {
        var command = new ResetPasswordCommand(
            Guid.Empty,
            "valid-looking-token-string",
            "Strong1!x",
            "Strong1!x");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.UserId));
    }

    [Fact]
    public async Task Should_fail_when_token_empty()
    {
        var command = new ResetPasswordCommand(
            Guid.NewGuid(),
            string.Empty,
            "Strong1!x",
            "Strong1!x");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.Token));
    }

    [Theory]
    [InlineData("short1!")]
    public async Task Should_fail_when_password_weak(string weak)
    {
        var command = new ResetPasswordCommand(
            Guid.NewGuid(),
            "token",
            weak,
            weak);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.Password));
    }

    [Fact]
    public async Task Should_fail_when_passwords_mismatch()
    {
        var command = new ResetPasswordCommand(
            Guid.NewGuid(),
            "token-value",
            "Strong1!x",
            "Strong1!y");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.ConfirmPassword));
    }

    [Fact]
    public async Task Should_pass_when_valid()
    {
        var id = Guid.NewGuid();
        var command = new ResetPasswordCommand(
            id,
            "some-reset-token-from-identity",
            "Strong1!x",
            "Strong1!x");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}

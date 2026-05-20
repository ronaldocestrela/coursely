using System.Security.Cryptography;
using Application.Features.Auth.Commands.Logout;
using FluentAssertions;

namespace UnitTests;

public sealed class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_refresh_token_should_fail(string token)
    {
        var command = new LogoutCommand(token);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_logout_should_pass()
    {
        var buf = new byte[16];
        RandomNumberGenerator.Fill(buf);

        var command = new LogoutCommand(Convert.ToBase64String(buf));
        var result = _sut.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}

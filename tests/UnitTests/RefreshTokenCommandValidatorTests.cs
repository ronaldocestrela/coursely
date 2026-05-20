using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Refresh;
using FluentAssertions;

namespace UnitTests;

public sealed class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_refresh_token_should_fail(string token)
    {
        var command = new RefreshTokenCommand(token);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_refresh_should_pass()
    {
        var command = new RefreshTokenCommand(
            Convert.ToBase64String("opaque-refresh-sample-32bytesXXXX"u8.ToArray()));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}

using Application.Features.Auth.Commands.Login;
using FluentAssertions;
using Moq;
using Shared.Auth;
using Shared.Results;

namespace UnitTests;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Should_return_success_when_service_succeeds()
    {
        var command = new LoginCommand("jane@example.com", "Strong1!x");
        var response = new LoginResponse(
            Guid.NewGuid(),
            "Jane",
            "jane@example.com",
            [],
            "access-token",
            DateTimeOffset.UtcNow.AddMinutes(60),
            "refresh-token",
            DateTimeOffset.UtcNow.AddDays(7));

        var mock = new Mock<IUserLoginService>();
        mock.Setup(s => s.LoginAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        var handler = new LoginCommandHandler(mock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task Should_return_invalid_credentials_from_service()
    {
        var command = new LoginCommand("jane@example.com", "wrong");
        var mock = new Mock<IUserLoginService>();
        mock.Setup(s => s.LoginAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.InvalidCredentials, "bad")));

        var handler = new LoginCommandHandler(mock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(AuthErrorCodes.InvalidCredentials);
    }
}

using Application.Features.Auth.Commands.RegisterUser;
using FluentAssertions;
using Moq;
using Shared.Auth;
using Shared.Results;

namespace UnitTests;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Should_return_success_when_service_succeeds()
    {
        var command = new RegisterUserCommand("Jane", "jane@example.com", "Strong1!x");
        var response = new RegisterUserResponse(Guid.NewGuid(), "Jane", "jane@example.com");
        var mock = new Mock<IUserRegistrationService>();
        mock.Setup(s => s.RegisterAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        var handler = new RegisterUserCommandHandler(mock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task Should_return_duplicate_email_error_from_service()
    {
        var command = new RegisterUserCommand("Jane", "jane@example.com", "Strong1!x");
        var mock = new Mock<IUserRegistrationService>();
        mock.Setup(s => s.RegisterAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RegisterUserResponse>(
                new Error(AuthErrorCodes.DuplicateEmail, "dup")));

        var handler = new RegisterUserCommandHandler(mock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(AuthErrorCodes.DuplicateEmail);
    }
}

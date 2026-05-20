using Application.Features.Auth.Commands.ResetPassword;
using Application.Features.Auth.PasswordRecovery;
using FluentAssertions;
using Moq;
using Shared.Auth;
using Shared.Results;

namespace UnitTests;

public sealed class ResetPasswordCommandHandlerTests
{
    [Fact]
    public async Task Should_return_failure_when_service_reports_invalid()
    {
        var userId = Guid.NewGuid();
        var mock = new Mock<IUserPasswordRecoveryService>();
        mock.Setup(s => s.ResetPasswordAsync(userId, "t", "Strong1!x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(AuthErrorCodes.PasswordResetInvalid, "bad")));

        var handler = new ResetPasswordCommandHandler(mock.Object);
        var result = await handler.Handle(
            new ResetPasswordCommand(userId, "t", "Strong1!x", "Strong1!x"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(AuthErrorCodes.PasswordResetInvalid);
    }

    [Fact]
    public async Task Should_pass_through_success()
    {
        var mock = new Mock<IUserPasswordRecoveryService>();
        mock.Setup(s =>
                s.ResetPasswordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ResetPasswordCommandHandler(mock.Object);
        var result = await handler.Handle(
            new ResetPasswordCommand(Guid.NewGuid(), "tok", "Strong2!z", "Strong2!z"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

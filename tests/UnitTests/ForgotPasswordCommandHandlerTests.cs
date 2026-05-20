using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.PasswordRecovery;
using FluentAssertions;
using Moq;

namespace UnitTests;

public sealed class ForgotPasswordCommandHandlerTests
{
    [Fact]
    public async Task Should_always_return_generic_success_payload()
    {
        var mock = new Mock<IUserPasswordRecoveryService>();
        mock.Setup(s =>
                s.RequestPasswordResetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ForgotPasswordCommandHandler(mock.Object);
        var result = await handler.Handle(new ForgotPasswordCommand("found@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Message.Should().Be(ForgotPasswordMessages.GenericSuccess);

        mock.Verify(
            s => s.RequestPasswordResetAsync("found@example.com", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

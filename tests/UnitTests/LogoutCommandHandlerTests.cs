using Application.Features.Auth.Commands.Logout;
using FluentAssertions;
using Moq;
using Shared.Results;

namespace UnitTests;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Returns_success_when_logout_finishes()
    {
        var command = new LogoutCommand("refresh-token-plain");
        var svc = new Mock<IUserLogoutService>();
        svc.Setup(s => s.LogoutAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var sut = new LogoutCommandHandler(svc.Object);
        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}

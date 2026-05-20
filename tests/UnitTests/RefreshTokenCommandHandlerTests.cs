using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Refresh;
using FluentAssertions;
using Moq;
using Shared.Auth;
using Shared.Results;

namespace UnitTests;

public sealed class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Returns_success_when_service_succeeds()
    {
        var command = new RefreshTokenCommand("some-refresh-token");
        var resp = new LoginResponse(
            Guid.NewGuid(),
            "Bob",
            "bob@example.com",
            [],
            "access-token",
            DateTimeOffset.UtcNow.AddMinutes(15),
            "new-refresh",
            DateTimeOffset.UtcNow.AddDays(7));

        var svc = new Mock<IUserRefreshTokenService>();
        svc.Setup(s => s.RefreshAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(resp));

        var sut = new RefreshTokenCommandHandler(svc.Object);
        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(resp);
    }

    [Fact]
    public async Task Returns_refresh_errors_from_service()
    {
        var command = new RefreshTokenCommand("old-token");
        var svc = new Mock<IUserRefreshTokenService>();
        svc.Setup(s => s.RefreshAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(
            Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.RefreshTokenReuseDetected, "reuse")));

        var sut = new RefreshTokenCommandHandler(svc.Object);
        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(AuthErrorCodes.RefreshTokenReuseDetected);
    }
}

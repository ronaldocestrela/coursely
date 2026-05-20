using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(IUserLogoutService logoutService)
    : IRequestHandler<LogoutCommand, Result>
{
    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken) =>
        logoutService.LogoutAsync(request, cancellationToken);
}

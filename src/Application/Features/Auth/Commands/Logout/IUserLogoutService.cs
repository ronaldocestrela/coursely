using Shared.Results;

namespace Application.Features.Auth.Commands.Logout;

public interface IUserLogoutService
{
    Task<Result> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken);
}

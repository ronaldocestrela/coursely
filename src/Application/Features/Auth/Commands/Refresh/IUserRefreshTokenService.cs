using Application.Features.Auth.Commands.Login;
using Shared.Results;

namespace Application.Features.Auth.Commands.Refresh;

public interface IUserRefreshTokenService
{
    Task<Result<LoginResponse>> RefreshAsync(RefreshTokenCommand command, CancellationToken cancellationToken);
}

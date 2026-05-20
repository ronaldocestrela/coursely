using Shared.Results;

namespace Application.Features.Auth.Commands.Login;

public interface IUserLoginService
{
    Task<Result<LoginResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
}

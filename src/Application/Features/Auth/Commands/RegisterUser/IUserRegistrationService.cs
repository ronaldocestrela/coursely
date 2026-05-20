using Shared.Results;

namespace Application.Features.Auth.Commands.RegisterUser;

public interface IUserRegistrationService
{
    Task<Result<RegisterUserResponse>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken);
}

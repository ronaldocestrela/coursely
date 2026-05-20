using Shared.Results;

namespace Application.Features.Auth.PasswordRecovery;

public interface IUserPasswordRecoveryService
{
    /// <summary>
    /// Request a password reset for the given email. Does not reveal whether the account exists.
    /// </summary>
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken);
}

namespace Application.Features.Auth.PasswordRecovery;

public interface IPasswordResetEmailSender
{
    Task SendPasswordResetAsync(
        string email,
        Guid userId,
        string resetLink,
        CancellationToken cancellationToken);
}

using Application.Features.Auth.PasswordRecovery;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

/// <summary>
/// Local/dev implementation that does not deliver e-mail: logs a structured payload with the reset link.
/// </summary>
public sealed class PasswordResetLoggingEmailSender(ILogger<PasswordResetLoggingEmailSender> logger)
    : IPasswordResetEmailSender
{
    public Task SendPasswordResetAsync(
        string email,
        Guid userId,
        string resetLink,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Password reset requested. RecipientEmail={RecipientEmail}, UserId={UserId}, ResetLink={ResetLink}",
            email,
            userId,
            resetLink);

        return Task.CompletedTask;
    }
}

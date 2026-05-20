using Application.Features.Auth.PasswordRecovery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Auth;
using Shared.Results;

namespace Infrastructure.Identity;

public sealed class UserPasswordRecoveryService(
    UserManager<ApplicationUser> userManager,
    IPasswordResetEmailSender emailSender,
    IOptions<PasswordRecoveryOptions> options,
    ILogger<UserPasswordRecoveryService> logger)
    : IUserPasswordRecoveryService
{
    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var normalizedEmail = userManager.NormalizeEmail(email.Trim());
            if (string.IsNullOrEmpty(normalizedEmail))
            {
                return;
            }

            var user = await userManager
                .FindByEmailAsync(normalizedEmail)
                .ConfigureAwait(false);

            /** Always exit quietly when unknown e-mail — prevents enumeration. */
            if (user is null)
            {
                return;
            }

            var token = await userManager
                .GeneratePasswordResetTokenAsync(user)
                .ConfigureAwait(false);

            var baseUrl = (options.Value.FrontendBaseUrl ?? string.Empty).TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogWarning("Password recovery FrontendBaseUrl is not configured; cannot build reset link.");
                return;
            }

            var encodedToken = Uri.EscapeDataString(token);
            var resetLink = $"{baseUrl}/redefinir-senha?userId={user.Id}&token={encodedToken}";

            var recipient = user.Email ?? normalizedEmail;
            await emailSender
                .SendPasswordResetAsync(recipient, user.Id, resetLink, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            /** Preserve ambiguous client response — log server-side failures. */
            logger.LogError(ex, "Unhandled error during password reset request.");
        }
    }

    public async Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            return Result.Failure(
                new Error(
                    AuthErrorCodes.PasswordResetInvalid,
                    "Não foi possível redefinir a senha. Solicite um novo link."));
        }

        var user = await userManager
            .FindByIdAsync(userId.ToString("D"))
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(
                new Error(
                    AuthErrorCodes.PasswordResetInvalid,
                    "Não foi possível redefinir a senha. Solicite um novo link."));
        }

        var identityResult = await userManager
            .ResetPasswordAsync(user, token, newPassword)
            .ConfigureAwait(false);

        if (identityResult.Succeeded)
        {
            return Result.Success();
        }

        var description = string.Join(' ', identityResult.Errors.Select(e => e.Description));
        logger.LogWarning(
            "Password reset failed for {UserId}: {Errors}",
            userId,
            description);

        if (identityResult.Errors.Any(e => e.Code?.Contains("InvalidToken", StringComparison.OrdinalIgnoreCase) == true))
        {
            return Result.Failure(
                new Error(
                    AuthErrorCodes.PasswordResetInvalid,
                    "Link inválido ou expirado. Solicite uma nova recuperação de senha."));
        }

        return Result.Failure(
            new Error(
                AuthErrorCodes.PasswordResetFailed,
                string.IsNullOrWhiteSpace(description)
                    ? "Não foi possível redefinir a senha."
                    : description));
    }
}

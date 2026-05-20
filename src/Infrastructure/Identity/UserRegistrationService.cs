using Application.Features.Auth.Commands.RegisterUser;
using Microsoft.AspNetCore.Identity;
using Shared.Auth;
using Shared.Results;

namespace Infrastructure.Identity;

public sealed class UserRegistrationService(UserManager<ApplicationUser> userManager) : IUserRegistrationService
{
    public async Task<Result<RegisterUserResponse>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedEmail = userManager.NormalizeEmail(command.Email.Trim());
        if (string.IsNullOrEmpty(normalizedEmail))
        {
            return Result.Failure<RegisterUserResponse>(
                new Error(AuthErrorCodes.RegistrationFailed, "E-mail inválido."));
        }

        var existing = await userManager.FindByEmailAsync(normalizedEmail).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result.Failure<RegisterUserResponse>(
                new Error(AuthErrorCodes.DuplicateEmail, "Este e-mail já está em uso."));
        }

        var now = DateTime.UtcNow;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = false,
            Name = command.Name.Trim(),
            Bio = null,
            AvatarUrl = null,
            IsPublicProfile = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var identityResult = await userManager.CreateAsync(user, command.Password).ConfigureAwait(false);
        if (!identityResult.Succeeded)
        {
            var message = string.Join(
                ' ',
                identityResult.Errors.Select(e => e.Description));
            return Result.Failure<RegisterUserResponse>(
                new Error(AuthErrorCodes.RegistrationFailed, string.IsNullOrWhiteSpace(message)
                    ? "Não foi possível criar a conta."
                    : message));
        }

        return Result.Success(new RegisterUserResponse(user.Id, user.Name, user.Email!));
    }
}

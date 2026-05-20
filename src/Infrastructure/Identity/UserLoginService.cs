using System.Security.Cryptography;
using System.Text;
using Application.Features.Auth.Commands.Login;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Auth;
using Shared.Results;

namespace Infrastructure.Identity;

public sealed class UserLoginService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    JwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions)
    : IUserLoginService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<LoginResponse>> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var trimmedEmail = command.Email.Trim();
        var normalizedEmail = userManager.NormalizeEmail(trimmedEmail);
        if (string.IsNullOrEmpty(normalizedEmail))
        {
            return Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.InvalidCredentials, "E-mail ou senha inválidos."));
        }

        var user = await userManager.FindByEmailAsync(normalizedEmail).ConfigureAwait(false);
        if (user is null)
        {
            return Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.InvalidCredentials, "E-mail ou senha inválidos."));
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password).ConfigureAwait(false);
        if (!passwordValid)
        {
            return Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.InvalidCredentials, "E-mail ou senha inválidos."));
        }

        var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        IReadOnlyList<string> roleList = roles.Count == 0
            ? []
            : roles.ToList();

        var (accessToken, accessExpires) = jwtTokenService.CreateAccessToken(
            user.Id,
            user.Email ?? normalizedEmail,
            user.Name,
            roleList);

        var refreshPlain = RefreshTokenGenerator.CreateOpaqueToken();
        var refreshHash = TokenHasher.Hash(refreshPlain);
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpires,
            CreatedAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(
            new LoginResponse(
                user.Id,
                user.Name,
                user.Email ?? normalizedEmail,
                roleList,
                accessToken,
                accessExpires,
                refreshPlain,
                new DateTimeOffset(DateTime.SpecifyKind(refreshExpires, DateTimeKind.Utc))));
    }
}

internal static class RefreshTokenGenerator
{
    public static string CreateOpaqueToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}

internal static class TokenHasher
{
    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}

using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Refresh;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Auth;
using Shared.Results;

namespace Infrastructure.Identity;

public sealed class UserRefreshTokenService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    JwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions)
    : IUserRefreshTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<LoginResponse>> RefreshAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var refreshPlain = command.RefreshToken.Trim();
        var refreshHash = TokenHasher.Hash(refreshPlain);

        var stored = await dbContext.RefreshTokens
            .Include(t => t.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.TokenHash == refreshHash, cancellationToken)
            .ConfigureAwait(false);

        if (stored is null)
        {
            return Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.RefreshTokenInvalid, "Refresh token inválido."));
        }

        var utcNow = DateTime.UtcNow;

        if (stored.RevokedAt.HasValue)
        {
            stored.ReuseDetectedAt ??= utcNow;
            await RevokeActiveFamilyTokensAsync(stored.UserId, stored.FamilyId, utcNow, cancellationToken)
                .ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Failure<LoginResponse>(
                new Error(
                    AuthErrorCodes.RefreshTokenReuseDetected,
                    "Uso inválido do refresh token detectado. Faça login novamente."));
        }

        if (stored.ExpiresAt <= utcNow)
        {
            return Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.RefreshTokenExpired, "Refresh token expirado. Faça login novamente."));
        }

        if (await userManager.IsLockedOutAsync(stored.User).ConfigureAwait(false))
        {
            return Result.Failure<LoginResponse>(
                new Error(AuthErrorCodes.RefreshTokenInvalid, "Refresh token inválido."));
        }

        var roles = await userManager.GetRolesAsync(stored.User).ConfigureAwait(false);
        IReadOnlyList<string> roleList = roles.Count == 0
            ? []
            : roles.ToList();

        var normalizedEmail =
            stored.User.Email is null
                ? (await userManager.GetEmailAsync(stored.User).ConfigureAwait(false)) ?? string.Empty
                : stored.User.Email;

        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var (accessToken, accessExpiresAt) = jwtTokenService.CreateAccessToken(
                stored.UserId,
                normalizedEmail,
                stored.User.Name,
                roleList);

            var newPlain = RefreshTokenGenerator.CreateOpaqueToken();
            var newHash = TokenHasher.Hash(newPlain);
            var newTokenId = Guid.NewGuid();

            stored.RevokedAt = utcNow;
            stored.ReplacedByTokenId = newTokenId;

            var refreshExpires = utcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);

            dbContext.RefreshTokens.Add(new RefreshToken
            {
                Id = newTokenId,
                UserId = stored.UserId,
                FamilyId = stored.FamilyId,
                TokenHash = newHash,
                ExpiresAt = refreshExpires,
                CreatedAt = utcNow,
            });

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success(
                new LoginResponse(
                    stored.UserId,
                    stored.User.Name,
                    normalizedEmail,
                    roleList,
                    accessToken,
                    accessExpiresAt,
                    newPlain,
                    new DateTimeOffset(DateTime.SpecifyKind(refreshExpires, DateTimeKind.Utc))));
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private async Task RevokeActiveFamilyTokensAsync(
        Guid userId,
        Guid familyId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.RevokedAt, utcNow),
                cancellationToken)
            .ConfigureAwait(false);
    }
}

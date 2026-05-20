using Application.Features.Auth.Commands.Logout;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Results;

namespace Infrastructure.Identity;

public sealed class UserLogoutService(ApplicationDbContext dbContext)
    : IUserLogoutService
{
    public async Task<Result> LogoutAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var trimmed = command.RefreshToken.Trim();

        var hash = TokenHasher.Hash(trimmed);

        var stored = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken)
            .ConfigureAwait(false);

        if (stored is null)
        {
            return Result.Success();
        }

        var utcNow = DateTime.UtcNow;

        if (stored.RevokedAt.HasValue)
        {
            return Result.Success();
        }

        stored.RevokedAt = utcNow;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

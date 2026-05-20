using Infrastructure.Identity;

namespace Infrastructure.Persistence.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// SHA-256 (Base64) of the opaque refresh token returned to the client.
    /// </summary>
    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

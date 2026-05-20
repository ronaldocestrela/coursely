using Infrastructure.Identity;

namespace Infrastructure.Persistence.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Identifies related refresh tokens in the rotation chain from a single login (session lineage).
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// SHA-256 (Base64) of the opaque refresh token returned to the client.
    /// </summary>
    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC time when this token was rotated out or invalidated (logout etc.).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// When revoked due to rotation, points to the replacement token row id.
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    public RefreshToken? ReplacedByToken { get; set; }

    /// <summary>
    /// Set when client presents a revoked token (possible theft / replay).
    /// </summary>
    public DateTime? ReuseDetectedAt { get; set; }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

    public bool IsActive(DateTime utcNow) => RevokedAt is null && ExpiresAt > utcNow;
}

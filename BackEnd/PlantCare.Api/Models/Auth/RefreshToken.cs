using PlantCare.Api.Models;

namespace PlantCare.Api.Models.Auth;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    // SHA-256 hash of the actual opaque refresh token value - never store the raw token.
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    // Rotation chain: when this token is used to issue a new one, that new token's id goes here,
    // so a family of rotated tokens can be traced and revoked together on reuse detection.
    public int? ReplacedByRefreshTokenId { get; set; }
}

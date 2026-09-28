using PlantCare.Api.DTOs.Users;

namespace PlantCare.Api.Services.Interfaces.Auth;

public enum RefreshTokenStatus
{
    Success,
    Invalid,
    Expired,
    ReuseDetected
}

public class RefreshResult
{
    public RefreshTokenStatus Status { get; init; }

    public string? AccessToken { get; init; }

    public string? RawRefreshToken { get; init; }

    public DateTime? RefreshTokenExpiresAt { get; init; }

    public UserDto? User { get; init; }

    public static RefreshResult Ok(string accessToken, string rawRefreshToken, DateTime refreshTokenExpiresAt, UserDto user) =>
        new()
        {
            Status = RefreshTokenStatus.Success,
            AccessToken = accessToken,
            RawRefreshToken = rawRefreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = user
        };

    public static RefreshResult Fail(RefreshTokenStatus status) => new() { Status = status };
}

public interface IRefreshTokenService
{
    // Issues a brand-new refresh token row for the given user and returns the raw (unhashed) value —
    // that raw value is never persisted, only its SHA-256 hash. Used from RegisterAsync/LoginAsync
    // and internally by RefreshAsync's rotation step.
    Task<(string RawToken, DateTime ExpiresAt)> IssueAsync(int userId, CancellationToken cancellationToken = default);

    // Validates a raw refresh token and, on success, rotates it: revokes the presented token, issues
    // a replacement (chained via RefreshToken.ReplacedByRefreshTokenId), and mints a new access token
    // for the same user. Reuse of an already-revoked token revokes the user's entire active token set
    // (see RefreshTokenService) — the standard defense against a stolen/replayed refresh token.
    Task<RefreshResult> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default);

    // Idempotent: revoking an unknown or already-revoked token is a no-op, never throws — logout must
    // succeed even against a stale or missing cookie.
    Task RevokeAsync(string rawRefreshToken, CancellationToken cancellationToken = default);
}

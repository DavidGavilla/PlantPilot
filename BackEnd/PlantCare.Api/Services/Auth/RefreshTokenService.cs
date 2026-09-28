using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Data;
using PlantCare.Api.Mappers;
using PlantCare.Api.Models.Auth;
using PlantCare.Api.Services.Interfaces.Auth;

namespace PlantCare.Api.Services.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    // 30 days — long-lived by design now that a short-lived access token (Jwt:ExpiryMinutes) exists
    // to limit exposure of the token that actually rides on every API call.
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    // 64 random bytes, base64url-encoded — an opaque secret, not a JWT. The client never parses it,
    // it only needs to be unguessable and comparable by hash.
    private const int RawTokenByteLength = 64;

    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenService(AppDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<(string RawToken, DateTime ExpiresAt)> IssueAsync(int userId, CancellationToken cancellationToken = default)
    {
        var (raw, entity) = await CreateAndPersistAsync(userId, cancellationToken);
        return (raw, entity.ExpiresAt);
    }

    public async Task<RefreshResult> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        var hash = Hash(rawRefreshToken);

        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existing == null)
            return RefreshResult.Fail(RefreshTokenStatus.Invalid);

        if (existing.RevokedAt != null)
        {
            // SECURITY: reuse of an already-revoked refresh token is the standard signal of token
            // theft — the legitimate client already rotated past this exact token, so whoever
            // presented it again cannot be the legitimate client. Revoke every currently-active
            // token for this user (kills the whole family, not just this one request) and force a
            // full re-login rather than trusting the request.
            await RevokeAllActiveForUserAsync(existing.UserId, cancellationToken);
            return RefreshResult.Fail(RefreshTokenStatus.ReuseDetected);
        }

        if (existing.ExpiresAt < DateTime.UtcNow)
            return RefreshResult.Fail(RefreshTokenStatus.Expired);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == existing.UserId, cancellationToken);

        // The owning user was removed out from under an otherwise-valid refresh token (shouldn't
        // happen — RefreshToken cascades from User — but treated as invalid rather than throwing).
        if (user == null)
            return RefreshResult.Fail(RefreshTokenStatus.Invalid);

        var (rawNew, newEntity) = await CreateAndPersistAsync(existing.UserId, cancellationToken);

        // Atomic, conditional revoke-and-link: the WHERE clause (RevokedAt == null) is evaluated by
        // SQL Server as part of the UPDATE itself, not as a separate read, so this closes the race
        // where two concurrent requests both read RevokedAt == null above and would otherwise both
        // proceed to rotate the same still-valid token. Only the request whose UPDATE actually
        // matches a row (rowsUpdated == 1) is the legitimate winner of the rotation.
        var rowsUpdated = await _context.RefreshTokens
            .Where(t => t.RefreshTokenId == existing.RefreshTokenId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                .SetProperty(t => t.ReplacedByRefreshTokenId, newEntity.RefreshTokenId), cancellationToken);

        if (rowsUpdated == 0)
        {
            // Lost the race: something else (a concurrent legitimate request, or an actual replay)
            // already rotated this exact token in the gap between our read and our write. Treat this
            // exactly like reuse detection — revoke every active token for this user. That sweep
            // naturally includes the child token just minted above (it's still unrevoked at this
            // point), so the loser never hands back a live, usable token even though it was created.
            await RevokeAllActiveForUserAsync(existing.UserId, cancellationToken);
            return RefreshResult.Fail(RefreshTokenStatus.ReuseDetected);
        }

        var accessToken = _jwtTokenService.GenerateToken(user);

        return RefreshResult.Ok(accessToken, rawNew, newEntity.ExpiresAt, user.ToDto());
    }

    public async Task RevokeAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        var hash = Hash(rawRefreshToken);

        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        // Idempotent: unknown/already-revoked tokens are a silent no-op — logout must always succeed
        // from the caller's perspective, even against a stale or already-used cookie.
        if (existing == null || existing.RevokedAt != null)
            return;

        existing.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeAllActiveForUserAsync(int userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var revokedAt = DateTime.UtcNow;
        foreach (var token in activeTokens)
            token.RevokedAt = revokedAt;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<(string RawToken, RefreshToken Entity)> CreateAndPersistAsync(int userId, CancellationToken cancellationToken)
    {
        var raw = GenerateRawToken();

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = Hash(raw),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime)
        };

        _context.RefreshTokens.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return (raw, entity);
    }

    private static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(RawTokenByteLength))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}

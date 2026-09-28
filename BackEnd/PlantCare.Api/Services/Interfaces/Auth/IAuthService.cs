using PlantCare.Api.DTOs.Users;

namespace PlantCare.Api.Services.Interfaces.Auth;

public enum AuthServiceStatus
{
    Success,
    EmailAlreadyTaken,
    InvalidCredentials
}

public class AuthResult
{
    public AuthServiceStatus Status { get; init; }

    public string? Token { get; init; }

    // Raw (unhashed) refresh token value + its expiry, issued alongside the access token so the
    // controller can set both session cookies in one round trip. Never persisted as-is (see
    // RefreshTokenService) — only its hash is stored.
    public string? RefreshToken { get; init; }

    public DateTime? RefreshTokenExpiresAt { get; init; }

    public UserDto? User { get; init; }

    public static AuthResult Ok(string token, string refreshToken, DateTime refreshTokenExpiresAt, UserDto user) =>
        new()
        {
            Status = AuthServiceStatus.Success,
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = user
        };

    public static AuthResult EmailTaken() =>
        new() { Status = AuthServiceStatus.EmailAlreadyTaken };

    public static AuthResult InvalidCredentials() =>
        new() { Status = AuthServiceStatus.InvalidCredentials };
}

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken = default);

    // Backs GET /api/auth/me — the frontend's only way to discover the current caller now that the
    // token lives in an httpOnly cookie it can never read directly.
    Task<UserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}

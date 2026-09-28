using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.DTOs.Users;
using PlantCare.Api.Services.Interfaces.Auth;

namespace PlantCare.Api.Controllers;

// No [Authorize] on Register/Login/Logout — these are the entry points that hand out or clear a
// session in the first place, and logging out an already-unauthenticated caller is harmless. Me
// requires [Authorize]: it's the frontend's only way to discover who is logged in, since the
// session token now lives in an httpOnly cookie page JavaScript can never read.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // The session lives only in this httpOnly cookie — never in the JSON body or localStorage.
    // The frontend talks to the API through the Vite dev proxy (same-origin in practice), so
    // SameSite=Lax is sufficient; cookie-only transport (no Authorization-header fallback) is used
    // because nothing in this app needs cross-site delivery.
    private const string SessionCookieName = "plantpilot_session";

    // Deliberately narrower Path than the session cookie (Path=/api/auth, not Path=/) — the refresh
    // token should only ever be sent to the auth endpoints that actually use it, not on every API
    // call, limiting how often it travels and where it could leak.
    private const string RefreshCookieName = "plantpilot_refresh";
    private const string RefreshCookiePath = "/api/auth";

    private readonly IAuthService _authService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IRefreshTokenService refreshTokenService, IConfiguration configuration)
    {
        _authService = authService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);

        if (result.Status == AuthServiceStatus.EmailAlreadyTaken)
            return Conflict("Email is already registered.");

        AppendSessionCookie(result.Token!);
        AppendRefreshCookie(result.RefreshToken!, result.RefreshTokenExpiresAt!.Value);

        // No GetById-style action exists for a freshly registered user (auth has no "get current
        // user" endpoint yet), so there is no meaningful Location URI to point CreatedAtAction at —
        // a plain 201 body is used instead. The token never appears in the body anymore.
        return StatusCode(StatusCodes.Status201Created, result.User);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);

        if (result.Status == AuthServiceStatus.InvalidCredentials)
            return Unauthorized("Invalid email or password.");

        AppendSessionCookie(result.Token!);
        AppendRefreshCookie(result.RefreshToken!, result.RefreshTokenExpiresAt!.Value);

        return Ok(result.User);
    }

    // No [Authorize] — the whole point of this endpoint is to renew an access token the caller may
    // no longer hold (it expired), using only the longer-lived refresh cookie.
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var rawRefreshToken) || string.IsNullOrEmpty(rawRefreshToken))
            return Unauthorized();

        var result = await _refreshTokenService.RefreshAsync(rawRefreshToken, cancellationToken);

        if (result.Status != RefreshTokenStatus.Success)
        {
            // Invalid, expired, or (security-relevant) reused-token-detected — in every failure case
            // the caller must re-authenticate from scratch, so clear both cookies rather than leaving
            // a now-untrustworthy refresh cookie sitting in the browser.
            ClearAuthCookies();
            return Unauthorized();
        }

        AppendSessionCookie(result.AccessToken!);
        AppendRefreshCookie(result.RawRefreshToken!, result.RefreshTokenExpiresAt!.Value);

        return Ok(result.User);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        // Actually revoke the refresh token server-side (not just clear the cookie client-side) —
        // that's the whole point of storing hashes: logout has a real effect even if the cookie is
        // somehow replayed later. RevokeAsync is idempotent, so a missing/stale cookie is harmless.
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var rawRefreshToken) && !string.IsNullOrEmpty(rawRefreshToken))
            await _refreshTokenService.RevokeAsync(rawRefreshToken, cancellationToken);

        ClearAuthCookies();

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var callerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(callerIdClaim, out var callerId))
            return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(callerId, cancellationToken);

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }

    private void AppendSessionCookie(string token)
    {
        // Reuse the same Jwt:ExpiryMinutes value JwtTokenService already reads (single source of
        // truth) rather than hardcoding a second copy here.
        var expiryMinutes = _configuration.GetSection("Jwt").GetValue<int?>("ExpiryMinutes")
            ?? throw new InvalidOperationException("Jwt:ExpiryMinutes is not configured.");

        Response.Cookies.Append(SessionCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
            Path = "/"
        });
    }

    private void AppendRefreshCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(expiresAtUtc, TimeSpan.Zero),
            Path = RefreshCookiePath
        });
    }

    private void ClearAuthCookies()
    {
        // Delete() re-issues each cookie with matching Path already expired, which is what actually
        // clears it browser-side — the Path must match how the cookie was set, or the browser treats
        // it as a different cookie and never clears the original.
        Response.Cookies.Delete(SessionCookieName, new CookieOptions { Path = "/" });
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = RefreshCookiePath });
    }
}

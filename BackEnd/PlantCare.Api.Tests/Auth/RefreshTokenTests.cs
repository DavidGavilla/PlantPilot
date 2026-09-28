using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Users;
using PlantCare.Api.Models.Auth;
using PlantCare.Api.Tests.Fixtures;

namespace PlantCare.Api.Tests.Auth;

// Refresh-token rotation + reuse-detection integration tests. Unlike Plants/* tests (which mint a
// session token directly via AuthTestHelper, bypassing login — see that file's own comment), these
// exercise the real register/login/refresh/logout HTTP flow end to end, since a real Set-Cookie
// response is the only way to get a raw refresh-token value to test rotation/reuse against.
//
// Every client here is created with HandleCookies = false and cookies are attached manually as a
// raw "Cookie" header (matching AuthTestHelper's own style) — this keeps full control over exactly
// which cookie value is sent on each request, which the replay-attack scenario in
// Refresh_WithRevokedToken_ReturnsUnauthorized_AndDetectsReuse depends on.
public class RefreshTokenTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private const string SessionCookieName = "plantpilot_session";
    private const string RefreshCookieName = "plantpilot_refresh";

    [Fact]
    public async Task Login_IssuesBothAccessAndRefreshCookies()
    {
        // Arrange
        var client = CreateClient();
        var (email, password) = await RegisterAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginUserDto
        {
            Email = email,
            Password = password
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sessionCookie = GetRawSetCookie(response, SessionCookieName);
        var refreshCookie = GetRawSetCookie(response, RefreshCookieName);

        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", refreshCookie, StringComparison.OrdinalIgnoreCase);
        // The refresh cookie's Path must be narrower than the session cookie's (Path=/) — it should
        // only ever travel to the auth endpoints, not every API call.
        Assert.Contains("path=/api/auth", refreshCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_IssuesNewAccessToken_AndRotatesRefreshCookie()
    {
        // Arrange
        var client = CreateClient();
        var (_, _, registerResponse) = await RegisterWithCredentialsAsync(client);
        var originalRefreshToken = ExtractCookieValue(registerResponse, RefreshCookieName);

        // Act
        var response = await SendWithCookieAsync(client, "/api/auth/refresh", RefreshCookieName, originalRefreshToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var newAccessToken = ExtractCookieValue(response, SessionCookieName);
        var newRefreshToken = ExtractCookieValue(response, RefreshCookieName);

        // A new access token cookie is issued on every refresh (proven by its presence below); it is
        // NOT asserted to differ byte-for-byte from the original here — the JWT's own "exp" claim has
        // only second-level granularity, so a register+refresh happening within the same UTC second
        // can legitimately produce an identical token string despite being freshly minted. The
        // refresh token, by contrast, is 64 bytes of fresh randomness every time and is the
        // security-critical value proving rotation actually happened — assert that one strictly.
        Assert.False(string.IsNullOrWhiteSpace(newAccessToken));
        Assert.NotEqual(originalRefreshToken, newRefreshToken);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ReturnsUnauthorized_AndDetectsReuse()
    {
        // Arrange — refresh once (legitimate rotation): this revokes the original token and issues a
        // brand-new one.
        var client = CreateClient();
        var (_, _, registerResponse) = await RegisterWithCredentialsAsync(client);
        var originalRefreshToken = ExtractCookieValue(registerResponse, RefreshCookieName);

        var firstRefreshResponse = await SendWithCookieAsync(client, "/api/auth/refresh", RefreshCookieName, originalRefreshToken);
        Assert.Equal(HttpStatusCode.OK, firstRefreshResponse.StatusCode);
        var legitimateRotatedToken = ExtractCookieValue(firstRefreshResponse, RefreshCookieName);

        // Act — replay the original (now-revoked) refresh token, simulating a stolen/replayed token.
        var replayResponse = await SendWithCookieAsync(client, "/api/auth/refresh", RefreshCookieName, originalRefreshToken);

        // Assert — the replay itself is rejected.
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        // Assert — reuse detection revoked the WHOLE family, including the token that was issued by
        // the legitimate first refresh (not just this one bad request) — verified directly in the DB.
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var legitimateTokenRow = await db.RefreshTokens
            .AsNoTracking()
            .SingleAsync(t => t.TokenHash == Hash(legitimateRotatedToken));

        Assert.NotNull(legitimateTokenRow.RevokedAt);

        // A subsequent refresh attempt with the legitimately-rotated token must also now fail.
        var followUpResponse = await SendWithCookieAsync(client, "/api/auth/refresh", RefreshCookieName, legitimateRotatedToken);
        Assert.Equal(HttpStatusCode.Unauthorized, followUpResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithConcurrentRequestsForSameToken_OnlyOneSucceeds_AndForcesFamilyRevocation()
    {
        // Arrange
        var client = CreateClient();
        var (_, _, registerResponse) = await RegisterWithCredentialsAsync(client);
        var originalRefreshToken = ExtractCookieValue(registerResponse, RefreshCookieName);

        // Act — fire two refresh requests presenting the SAME still-valid token without awaiting one
        // before starting the other, racing them against the read-then-write window in
        // RefreshTokenService.RefreshAsync that Fix 1 closes with a conditional ExecuteUpdateAsync.
        var firstTask = SendWithCookieAsync(client, "/api/auth/refresh", RefreshCookieName, originalRefreshToken);
        var secondTask = SendWithCookieAsync(client, "/api/auth/refresh", RefreshCookieName, originalRefreshToken);
        var responses = await Task.WhenAll(firstTask, secondTask);

        // Assert — exactly one request won the race and got back a usable new token; the other lost
        // and was correctly rejected (never rotate the same token twice).
        var okResponses = responses.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
        var unauthorizedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.Unauthorized).ToList();

        Assert.Single(okResponses);
        Assert.Single(unauthorizedResponses);

        // Assert — the winner's newly-issued child token was ALSO revoked, as a direct consequence of
        // the loser being treated as a genuine replay (family-wide revocation). This is the real proof
        // the fix works: not just "one wins, one loses," but "the loser forces full-family revocation,"
        // exactly as a real replay attack would, even though the loser presented a token that was still
        // valid at the moment it read it.
        var winnerNewRefreshToken = ExtractCookieValue(okResponses[0], RefreshCookieName);

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var winnerChildRow = await db.RefreshTokens
            .AsNoTracking()
            .SingleAsync(t => t.TokenHash == Hash(winnerNewRefreshToken));

        Assert.NotNull(winnerChildRow.RevokedAt);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken_SoItCannotBeUsedAfterward()
    {
        // Arrange
        var client = CreateClient();
        var (_, _, registerResponse) = await RegisterWithCredentialsAsync(client);
        var refreshToken = ExtractCookieValue(registerResponse, RefreshCookieName);

        // Act
        var logoutResponse = await SendWithCookieAsync(client, "/api/auth/logout", RefreshCookieName, refreshToken);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        // Assert — the now-revoked refresh token can no longer be used to obtain a new session.
        var refreshAfterLogout = await SendWithCookieAsync(client, "/api/auth/refresh", RefreshCookieName, refreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    private HttpClient CreateClient() =>
        fixture.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    private static async Task<(string Email, string Password)> RegisterAsync(HttpClient client)
    {
        var (email, password, _) = await RegisterWithCredentialsAsync(client);
        return (email, password);
    }

    private static async Task<(string Email, string Password, HttpResponseMessage Response)> RegisterWithCredentialsAsync(HttpClient client)
    {
        var email = $"refresh-{Guid.NewGuid():N}@test.local";
        const string password = "Passw0rd1";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterUserDto
        {
            Name = "Refresh",
            LastName = "Tester",
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (email, password, response);
    }

    private static async Task<HttpResponseMessage> SendWithCookieAsync(HttpClient client, string path, string cookieName, string cookieValue)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("Cookie", $"{cookieName}={cookieValue}");
        return await client.SendAsync(request);
    }

    private static string ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        var raw = GetRawSetCookie(response, cookieName);
        var start = cookieName.Length + 1;
        var end = raw.IndexOf(';');
        return end == -1 ? raw[start..] : raw[start..end];
    }

    private static string GetRawSetCookie(HttpResponseMessage response, string cookieName)
    {
        var hasCookies = response.Headers.TryGetValues("Set-Cookie", out var setCookies);
        Assert.True(hasCookies, $"Response has no Set-Cookie headers (expected '{cookieName}').");

        var match = setCookies!.FirstOrDefault(c => c.StartsWith($"{cookieName}=", StringComparison.Ordinal));
        Assert.False(match is null, $"No Set-Cookie header found for '{cookieName}'.");

        return match!;
    }

    // Mirrors RefreshTokenService's private Hash() exactly — needed here only to look a token row up
    // by its hash for direct DB assertions, never to bypass the service's own hashing.
    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}

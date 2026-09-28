using Microsoft.Extensions.DependencyInjection;
using PlantCare.Api.Models;
using PlantCare.Api.Services.Interfaces.Auth;

namespace PlantCare.Api.Tests.Fixtures;

// PlantsController etc. now require a real JWT (see Controllers/ApiControllerBase.cs) whose
// NameIdentifier claim matches the route's {userId}. Integration tests still seed real
// Users/Workspaces/WorkspaceMembers directly via AppDbContext (per testing.md) — this helper just
// mints a real token for the seeded caller using the app's own IJwtTokenService, so the token is
// signed/shaped exactly like a real login would produce.
//
// The JWT bearer middleware now reads the session from the "plantpilot_session" cookie instead of
// an Authorization header (see Program.cs), so tests attach it the same way a browser would: as a
// raw Cookie header on the HttpClient. Setting the header directly (rather than routing through
// WebApplicationFactory's CookieContainer) is the simpler option here since these tests never need
// the client to receive/re-send a Set-Cookie response — they just need to present the token.
public static class AuthTestHelper
{
    public const string SessionCookieName = "plantpilot_session";

    public static string CreateToken(ApiFixture fixture, int userId, string? email = null, string? name = null)
    {
        using var scope = fixture.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var user = new User
        {
            UserId = userId,
            Name = name ?? "Test",
            LastName = "User",
            Email = email ?? $"user{userId}@test.local"
        };

        return jwtTokenService.GenerateToken(user);
    }

    public static void AttachSessionCookie(
        HttpClient client, ApiFixture fixture, int userId, string? email = null, string? name = null)
    {
        var token = CreateToken(fixture, userId, email, name);

        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", $"{SessionCookieName}={token}");
    }
}

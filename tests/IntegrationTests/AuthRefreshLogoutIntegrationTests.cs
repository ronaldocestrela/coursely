using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public sealed class AuthRefreshLogoutIntegrationTests(
    IntegrationTestWebApplicationFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    [SkippableFact]
    public async Task Refresh_rotates_token_and_detects_old_token_reuse()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var email = $"refresh-{Guid.NewGuid():N}@example.com";
        const string password = "Strong1!x";

        var registered = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Refresh Tester", email, password });
        registered.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await login.Content.ReadFromJsonAsync<TokensPayload>(JsonOptions);
        loginBody.Should().NotBeNull();
        var rt1 = loginBody!.RefreshToken;

        var refresh1 = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = rt1 });
        refresh1.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed1 = await refresh1.Content.ReadFromJsonAsync<TokensPayload>(JsonOptions);
        refreshed1.Should().NotBeNull();

        refreshed1!.RefreshToken.Should().NotBe(rt1);

        /** Old refresh token reused after rotation (replay). */
        var replayOld = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = rt1 });
        replayOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        /** Current lineage should also be invalidated after replay detection (family revocation). */
        var attemptNew = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = refreshed1.RefreshToken });
        attemptNew.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task Logout_revokes_only_current_session_dual_login_stays_valid()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var clientA = factory.CreateClient();
        var clientB = factory.CreateClient();

        var email = $"dual-{Guid.NewGuid():N}@example.com";
        const string password = "Strong1!x";

        var registered = await clientA.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Dual User", email, password });
        registered.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginA = await clientA.PostAsJsonAsync("/api/auth/login", new { email, password });
        var loginB = await clientB.PostAsJsonAsync("/api/auth/login", new { email, password });

        loginA.StatusCode.Should().Be(HttpStatusCode.OK);
        loginB.StatusCode.Should().Be(HttpStatusCode.OK);

        var bodyA = await loginA.Content.ReadFromJsonAsync<TokensPayload>(JsonOptions);
        var bodyB = await loginB.Content.ReadFromJsonAsync<TokensPayload>(JsonOptions);
        bodyA.Should().NotBeNull();
        bodyB.Should().NotBeNull();

        var logoutResp = await clientA.PostAsJsonAsync(
            "/api/auth/logout",
            new { refreshToken = bodyA!.RefreshToken });
        logoutResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshLoggedOutSession = await clientA.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = bodyA.RefreshToken });
        refreshLoggedOutSession.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var refreshAlive = await clientB.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = bodyB!.RefreshToken });
        refreshAlive.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record TokensPayload(
        Guid UserId,
        string AccessToken,
        string RefreshToken);
}

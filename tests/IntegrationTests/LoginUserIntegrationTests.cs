using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Xunit;

namespace IntegrationTests;

public sealed class LoginUserIntegrationTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    [SkippableFact]
    public async Task Login_returns_tokens_and_me_matches_when_authorized()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var email = $"login-{Guid.NewGuid():N}@example.com";
        const string password = "Strong1!x";

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Login User", email, password });

        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await login.Content.ReadFromJsonAsync<LoginApiResponse>(JsonOptions);
        loginBody.Should().NotBeNull();
        loginBody!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginBody.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginBody.UserId.Should().NotBe(Guid.Empty);
        string.Equals(loginBody.Email, email, StringComparison.OrdinalIgnoreCase).Should().BeTrue();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);

        var me = await client.GetAsync("/api/auth/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK);
        var meBody = await me.Content.ReadFromJsonAsync<MeApiResponse>(JsonOptions);
        meBody.Should().NotBeNull();
        meBody!.UserId.Should().Be(loginBody.UserId);
        string.Equals(meBody.Email, email, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [SkippableFact]
    public async Task Login_returns_unauthorized_when_password_wrong()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var email = $"badlogin-{Guid.NewGuid():N}@example.com";
        const string password = "Strong1!x";

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Bad Login", email, password });
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = "Wrong1!x" });

        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record LoginApiResponse(Guid UserId, string AccessToken, string RefreshToken, string Email);

    private sealed record MeApiResponse(Guid UserId, string Email);
}

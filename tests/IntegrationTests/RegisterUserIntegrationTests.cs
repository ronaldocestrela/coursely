using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace IntegrationTests;

public sealed class RegisterUserIntegrationTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [SkippableFact]
    public async Task Register_returns_created_when_valid()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Integration User", email, password = "Strong1!x" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterUserApiResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Integration User");
        string.Equals(body.Email, email, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        body.Id.Should().NotBe(Guid.Empty);
    }

    [SkippableFact]
    public async Task Register_returns_conflict_when_email_duplicate()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var payload = new { name = "First", email, password = "Strong1!x" };

        var first = await client.PostAsJsonAsync("/api/auth/register", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/auth/register", payload);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private sealed record RegisterUserApiResponse(Guid Id, string Name, string Email);
}

using System.Net;
using FluentAssertions;

namespace IntegrationTests;

public sealed class HealthCheckTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task Health_reports_database_ready()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }
}

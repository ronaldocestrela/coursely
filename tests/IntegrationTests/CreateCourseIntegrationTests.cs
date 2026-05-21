using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public sealed class CreateCourseIntegrationTests(IntegrationTestWebApplicationFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    [SkippableFact]
    public async Task Create_returns_201_and_persists_for_authenticated_user()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var email = $"course-{Guid.NewGuid():N}@example.com";
        const string password = "Strong1!x";

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Course User", email, password });

        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await login.Content.ReadFromJsonAsync<LoginApiResponse>(JsonOptions);
        loginBody.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var create = await client.PostAsJsonAsync(
            "/api/courses",
            new
            {
                title = "Curso de teste",
                description = "Descrição",
                purchaseLink = "https://example.com/buy",
                thumbnailUrl = (string?)null,
                category = "Dev",
                visibility = "Public",
            });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<CreatedCourseApiResponse>(JsonOptions);
        created.Should().NotBeNull();
        created!.Title.Should().Be("Curso de teste");
        created.UserId.Should().Be(loginBody.UserId);
        created.Visibility.Should().Be("Public");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var inDb = await db.CourseWishlists.AsNoTracking().FirstOrDefaultAsync(c => c.Id == created.Id);
        inDb.Should().NotBeNull();
        inDb!.UserId.Should().Be(loginBody.UserId);
    }

    [SkippableFact]
    public async Task Create_returns_401_when_not_authenticated()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync(
            "/api/courses",
            new
            {
                title = "X",
                description = (string?)null,
                purchaseLink = (string?)null,
                thumbnailUrl = (string?)null,
                category = (string?)null,
                visibility = "Private",
            });

        create.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task Create_returns_400_when_title_invalid()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var email = $"course-bad-{Guid.NewGuid():N}@example.com";
        const string password = "Strong1!x";

        await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Bad", email, password });
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginApiResponse>(JsonOptions);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var create = await client.PostAsJsonAsync(
            "/api/courses",
            new
            {
                title = "",
                description = (string?)null,
                purchaseLink = (string?)null,
                thumbnailUrl = (string?)null,
                category = (string?)null,
                visibility = "Private",
            });

        create.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record LoginApiResponse(Guid UserId, string AccessToken, string RefreshToken, string Email);

    private sealed record CreatedCourseApiResponse(
        Guid Id,
        Guid UserId,
        string Title,
        string Visibility);
}

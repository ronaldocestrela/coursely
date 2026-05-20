using System.Text.Json;
using FluentAssertions;
using Application.Features.Auth.Commands.ForgotPassword;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shared.Auth;

namespace IntegrationTests;

public sealed class PasswordRecoveryIntegrationTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    [SkippableFact]
    public async Task Forgot_password_returns_same_generic_message_for_known_and_unknown_email()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();
        var randomEmail = $"unknown-{Guid.NewGuid():N}@example.com";
        var registeredEmail = $"known-{Guid.NewGuid():N}@example.com";
        const string password = "Strong1!x";

        var registerResp = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Known User", email = registeredEmail, password });
        registerResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var forgotUnknown = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new { email = randomEmail });
        forgotUnknown.StatusCode.Should().Be(HttpStatusCode.OK);

        var forgotKnown = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new { email = registeredEmail });
        forgotKnown.StatusCode.Should().Be(HttpStatusCode.OK);

        var unknownBody = await forgotUnknown.Content.ReadFromJsonAsync<ForgotPasswordEnvelope>(JsonOptions);
        var knownBody = await forgotKnown.Content.ReadFromJsonAsync<ForgotPasswordEnvelope>(JsonOptions);

        unknownBody.Should().NotBeNull();
        knownBody.Should().NotBeNull();

        unknownBody!.Message.Should().Be(ForgotPasswordMessages.GenericSuccess);
        unknownBody.Message.Should().Be(knownBody!.Message);
    }

    [SkippableFact]
    public async Task Reset_password_with_valid_identity_token_allows_login_with_new_secret()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();

        var email = $"reset-ok-{Guid.NewGuid():N}@example.com";
        var oldPassword = "Strong1!x";
        var newPassword = "Stronger2!y";

        var registerResp = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Reset Tester", email, password = oldPassword });
        registerResp.StatusCode.Should().Be(HttpStatusCode.Created);

        Guid userId;
        string resetToken;

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(userManager.NormalizeEmail(email));
            user.Should().NotBeNull();
            userId = user!.Id;
            resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        }

        var resetResp = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new
            {
                userId,
                token = resetToken,
                password = newPassword,
                confirmPassword = newPassword,
            });

        resetResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var loginOld = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = oldPassword });
        loginOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var loginNew = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = newPassword });
        loginNew.StatusCode.Should().Be(HttpStatusCode.OK);
        var logged = await loginNew.Content.ReadFromJsonAsync<LoginPeek>(JsonOptions);
        logged.Should().NotBeNull();
        logged!.UserId.Should().Be(userId);
    }

    [SkippableFact]
    public async Task Reset_password_returns_bad_request_for_invalid_token()
    {
        Skip.IfNot(factory.SqlServerIsAvailable, "Docker is required for SQL Server Testcontainers integration tests.");

        await factory.ResetDatabaseAsync();

        var client = factory.CreateClient();

        var email = $"reset-bad-{Guid.NewGuid():N}@example.com";
        var registerResp = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "Bad Token", email, password = "Strong1!x" });
        registerResp.StatusCode.Should().Be(HttpStatusCode.Created);

        Guid userId;

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(userManager.NormalizeEmail(email));
            userId = user!.Id;
        }

        var resetResp = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new
            {
                userId,
                token = "definitely-not-a-valid-token",
                password = "OtherStr0!ng",
                confirmPassword = "OtherStr0!ng",
            });

        resetResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var err = await resetResp.Content.ReadFromJsonAsync<ErrorEnvelope>(JsonOptions);
        err.Should().NotBeNull();
        err!.Code.Should().Be(AuthErrorCodes.PasswordResetInvalid);
    }

    private sealed record ForgotPasswordEnvelope(string Message);

    private sealed record LoginPeek(Guid UserId);

    private sealed record ErrorEnvelope(string Code, string Message);
}

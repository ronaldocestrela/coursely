using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using DotNet.Testcontainers.Builders;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using Testcontainers.MsSql;

namespace IntegrationTests;

/// <summary>
/// Spins up SQL Server via Testcontainers when Docker is available; otherwise runs API without DB wiring.
/// </summary>
public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Pinned Ubuntu-based tag (avoid floating <c>latest</c>); see MCR MSSQL artifact tags for updates.
    /// </summary>
    private const string MsSqlServerImage = "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";

    private MsSqlContainer? _sqlContainer;
    private Respawner? _respawner;

    /// <summary>
    /// When false, SQL-backed assertions should be skipped (e.g. Docker not running).
    /// </summary>
    public bool SqlServerIsAvailable { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");

        if (_sqlContainer is null)
        {
            return;
        }

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _sqlContainer.GetConnectionString(),
                    ["Cors:AllowedOrigins"] = "http://localhost",
                    ["Jwt:Key"] = "integration-test-jwt-signing-key-at-least-32-chars!!",
                    ["Jwt:Issuer"] = "Coursely.Tests",
                    ["Jwt:Audience"] = "Coursely.Tests",
                    ["Jwt:AccessTokenExpirationMinutes"] = "60",
                    ["Jwt:RefreshTokenExpirationDays"] = "7",
                    ["PasswordRecovery:FrontendBaseUrl"] = "http://localhost:5173",
                });
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            _sqlContainer = new MsSqlBuilder(MsSqlServerImage)
                .WithPassword("Test_password_123!")
                .WithWaitStrategy(
                    Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(
                        1433,
                        ws => ws
                            .WithTimeout(TimeSpan.FromMinutes(6))
                            .WithInterval(TimeSpan.FromSeconds(2))))
                .Build();

            await _sqlContainer.StartAsync();
        }
        catch (Exception ex) when (IsDockerOrContainerInfrastructureFailure(ex))
        {
            LogInfrastructureSkipReason(ex);
            await DisposeSqlContainerQuietlyAsync();
            SqlServerIsAvailable = false;
            return;
        }

        Debug.Assert(_sqlContainer is not null);

        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_sqlContainer.GetConnectionString())
                .Options;

            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
            }

            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_sqlContainer.GetConnectionString());
            await conn.OpenAsync();
            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions { DbAdapter = DbAdapter.SqlServer });

            SqlServerIsAvailable = true;
        }
        catch
        {
            await DisposeSqlContainerQuietlyAsync();
            throw;
        }
    }

    private static bool IsDockerOrContainerInfrastructureFailure(Exception ex)
    {
        foreach (var inner in IterateExceptions(ex))
        {
            switch (inner)
            {
                case DockerUnavailableException:
                    return true;
                case TimeoutException:
                case OperationCanceledException:
                case HttpRequestException:
                    return true;
                case IOException:
                    return true;
            }

            var name = inner.GetType().FullName ?? inner.GetType().Name;
            if (name.StartsWith("Docker.DotNet", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Exception> IterateExceptions(Exception ex)
    {
        if (ex is AggregateException agg)
        {
            foreach (var inner in agg.Flatten().InnerExceptions)
            {
                foreach (var e in IterateExceptions(inner))
                {
                    yield return e;
                }
            }

            yield break;
        }

        yield return ex;
        var walk = ex.InnerException;
        while (walk is not null)
        {
            yield return walk;
            walk = walk.InnerException;
        }
    }

    [SuppressMessage("Design", "CA1031")]
    private static void LogInfrastructureSkipReason(Exception ex)
    {
        try
        {
            Console.Error.WriteLine(
                $"[IntegrationTests] Skipping SQL Server Testcontainers ({nameof(SqlServerIsAvailable)}=false): {ex}");
        }
        catch
        {
            // Best-effort only.
        }
    }

    private async Task DisposeSqlContainerQuietlyAsync()
    {
        if (_sqlContainer is null)
        {
            return;
        }

        try
        {
            await _sqlContainer.DisposeAsync();
        }
        catch
        {
            // Ignore teardown failures when Docker never came up.
        }
        finally
        {
            _sqlContainer = null;
            _respawner = null;
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_sqlContainer is not null)
        {
            await _sqlContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Clears data between tests (schema kept via migrations).
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null || _sqlContainer is null)
        {
            throw new InvalidOperationException("Database reset requires Docker and SQL Server Testcontainers.");
        }

        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_sqlContainer.GetConnectionString());
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }
}

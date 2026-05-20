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
                });
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Test_password_123!")
                .Build();

            await _sqlContainer.StartAsync();

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
        catch (DockerUnavailableException)
        {
            await DisposeSqlContainerQuietlyAsync();
            SqlServerIsAvailable = false;
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

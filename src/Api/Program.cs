using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, _, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var corsOrigins = builder.Configuration["Cors:AllowedOrigins"]?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Coursely API",
        Version = "v1",
    });
});

var healthChecks = builder.Services.AddHealthChecks();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddDbContextCheck<ApplicationDbContext>("database");
}

// JWT Bearer package is referenced per roadmap; full validation lands in Phase 1.

var app = builder.Build();

if (!app.Environment.IsEnvironment("IntegrationTesting"))
{
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
        if (db is not null)
        {
            await db.Database.MigrateAsync();
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Coursely API v1");
    });
}

app.UseSerilogRequestLogging();

if (!app.Environment.IsEnvironment("IntegrationTesting"))
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

public partial class Program;

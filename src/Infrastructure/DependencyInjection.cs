using System.Text;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Refresh;
using Application.Features.Auth.Commands.RegisterUser;
using Application.Features.Auth.PasswordRecovery;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PasswordRecoveryOptions>(configuration.GetSection(PasswordRecoveryOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);

        // Always register EF Core + Identity + auth services so MediatR handlers (e.g. register) resolve.
        // Integration tests use WebApplicationFactory: connection string must be visible via env or merged config before host builds.
        // When no relational CS is configured, EF In-Memory is used (volatile). Program skips Migrate/EnsureCreated in IntegrationTesting.
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (hasConnectionString)
            {
                options.UseSqlServer(connectionString);
                return;
            }

            var label = environment.IsEnvironment("IntegrationTesting")
                ? "Coursely_Integration_NoRelationalCs"
                : environment.IsDevelopment()
                    ? "Coursely_DevLocal_NoConnectionString"
                    : "Coursely_Runtime_NoConnectionString";

            options.UseInMemoryDatabase(label);
        });

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = RegisterUserCommandValidator.MinimumPasswordLength;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        services.AddSingleton<JwtTokenService>();
        services.AddScoped<IUserLoginService, UserLoginService>();
        services.AddScoped<IUserRefreshTokenService, UserRefreshTokenService>();
        services.AddScoped<IUserLogoutService, UserLogoutService>();
        services.AddScoped<IPasswordResetEmailSender, PasswordResetLoggingEmailSender>();
        services.AddScoped<IUserPasswordRecoveryService, UserPasswordRecoveryService>();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtKey = jwtSection["Key"];
        if (!string.IsNullOrWhiteSpace(jwtKey))
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSection["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        NameClaimType = System.Security.Claims.ClaimTypes.Name,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    };
                });
        }

        return services;
    }
}

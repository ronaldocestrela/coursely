namespace IntegrationTests;

/// <summary>
/// Single source for integration-test host configuration so options binding and JWT bearer registration
/// (which reads configuration during service registration) see the same values as token issuance.
/// Process env mirrors the ASP.NET Core double-underscore environment variable convention.
/// </summary>
internal static class IntegrationHostSettings
{
    internal const string CorsAllowedOrigins = "http://localhost";
    internal const string JwtKey = "integration-test-jwt-signing-key-at-least-32-chars!!";
    internal const string JwtIssuer = "Coursely.Tests";
    internal const string JwtAudience = "Coursely.Tests";
    internal const string JwtAccessTokenExpirationMinutes = "60";
    internal const string JwtRefreshTokenExpirationDays = "7";
    internal const string PasswordRecoveryFrontendBaseUrl = "http://localhost:5173";

    private static readonly IReadOnlyList<string> ProcessEnvironmentExportKeys =
    [
        "ConnectionStrings__DefaultConnection",
        "Jwt__Key",
        "Jwt__Issuer",
        "Jwt__Audience",
        "Jwt__AccessTokenExpirationMinutes",
        "Jwt__RefreshTokenExpirationDays",
        "Cors__AllowedOrigins",
        "PasswordRecovery__FrontendBaseUrl",
    ];

    internal static Dictionary<string, string?> BuildAppConfigurationPairs(string sqlConnectionString) =>
        new()
        {
            ["ConnectionStrings:DefaultConnection"] = sqlConnectionString,
            ["Cors:AllowedOrigins"] = CorsAllowedOrigins,
            ["Jwt:Key"] = JwtKey,
            ["Jwt:Issuer"] = JwtIssuer,
            ["Jwt:Audience"] = JwtAudience,
            ["Jwt:AccessTokenExpirationMinutes"] = JwtAccessTokenExpirationMinutes,
            ["Jwt:RefreshTokenExpirationDays"] = JwtRefreshTokenExpirationDays,
            ["PasswordRecovery:FrontendBaseUrl"] = PasswordRecoveryFrontendBaseUrl,
        };

    internal static void ApplyToProcessEnvironment(string sqlConnectionString)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", sqlConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Key", JwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", JwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", JwtAudience);
        Environment.SetEnvironmentVariable("Jwt__AccessTokenExpirationMinutes", JwtAccessTokenExpirationMinutes);
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenExpirationDays", JwtRefreshTokenExpirationDays);
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins", CorsAllowedOrigins);
        Environment.SetEnvironmentVariable("PasswordRecovery__FrontendBaseUrl", PasswordRecoveryFrontendBaseUrl);
    }

    internal static void ClearProcessEnvironment()
    {
        foreach (var key in ProcessEnvironmentExportKeys)
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }
}

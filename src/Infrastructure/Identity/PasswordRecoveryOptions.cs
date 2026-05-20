namespace Infrastructure.Identity;

public sealed class PasswordRecoveryOptions
{
    public const string SectionName = "PasswordRecovery";

    /// <summary>
    /// Public web app base URL (no trailing slash) used to build the reset link e-mailed/logged to the user.
    /// </summary>
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}

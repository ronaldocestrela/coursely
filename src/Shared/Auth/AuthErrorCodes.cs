namespace Shared.Auth;

public static class AuthErrorCodes
{
    public const string DuplicateEmail = "auth.duplicate_email";
    public const string RegistrationFailed = "auth.registration_failed";
    public const string InvalidCredentials = "auth.invalid_credentials";

    public const string RefreshTokenInvalid = "auth.refresh_token_invalid";

    public const string RefreshTokenExpired = "auth.refresh_token_expired";

    public const string RefreshTokenReuseDetected = "auth.refresh_token_reuse_detected";
}

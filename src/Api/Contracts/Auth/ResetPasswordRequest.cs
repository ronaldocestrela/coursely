namespace Api.Contracts.Auth;

public sealed record ResetPasswordRequest(
    Guid UserId,
    string Token,
    string Password,
    string ConfirmPassword);

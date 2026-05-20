namespace Application.Features.Auth.Commands.Login;

public sealed record LoginResponse(
    Guid UserId,
    string Name,
    string Email,
    IReadOnlyList<string> Roles,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

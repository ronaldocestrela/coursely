namespace Api.Contracts.Auth;

public sealed record MeResponse(Guid UserId, string Email, string Name, IReadOnlyList<string> Roles);

namespace Api.Contracts.Auth;

public sealed record RegisterUserRequest(string Name, string Email, string Password);

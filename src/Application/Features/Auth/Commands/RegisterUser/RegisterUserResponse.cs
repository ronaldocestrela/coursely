namespace Application.Features.Auth.Commands.RegisterUser;

public sealed record RegisterUserResponse(Guid Id, string Name, string Email);

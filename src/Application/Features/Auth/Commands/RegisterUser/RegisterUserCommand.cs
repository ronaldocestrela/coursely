using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Name, string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;

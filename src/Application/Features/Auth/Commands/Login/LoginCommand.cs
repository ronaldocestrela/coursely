using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<Result<LoginResponse>>;

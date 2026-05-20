using Application.Features.Auth.Commands.Login;
using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.Refresh;

/// <summary>
/// Exchanges a valid refresh token for a new access + refresh pair (rotation).
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;

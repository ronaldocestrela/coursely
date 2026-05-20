using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.Logout;

/// <summary>
/// Revokes the supplied refresh token (current device session).
/// Idempotent — unknown or already revoked tokens still succeed server-side.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;

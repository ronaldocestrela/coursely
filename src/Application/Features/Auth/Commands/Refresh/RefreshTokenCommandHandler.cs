using Application.Features.Auth.Commands.Login;
using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler(IUserRefreshTokenService refreshTokenService)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken) =>
        refreshTokenService.RefreshAsync(request, cancellationToken);
}

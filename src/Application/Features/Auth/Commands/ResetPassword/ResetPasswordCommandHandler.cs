using Application.Features.Auth.PasswordRecovery;
using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(IUserPasswordRecoveryService recoveryService)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        => recoveryService.ResetPasswordAsync(request.UserId, request.Token, request.Password, cancellationToken);
}

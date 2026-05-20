using Application.Features.Auth.PasswordRecovery;
using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(IUserPasswordRecoveryService recoveryService)
    : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    public async Task<Result<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        await recoveryService
            .RequestPasswordResetAsync(request.Email.Trim(), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new ForgotPasswordResponse(ForgotPasswordMessages.GenericSuccess));
    }
}

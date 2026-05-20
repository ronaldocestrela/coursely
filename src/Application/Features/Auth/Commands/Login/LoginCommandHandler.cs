using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(IUserLoginService loginService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        loginService.LoginAsync(request, cancellationToken);
}

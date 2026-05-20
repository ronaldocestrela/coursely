using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(IUserRegistrationService registrationService)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    public Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
        => registrationService.RegisterAsync(request, cancellationToken);
}

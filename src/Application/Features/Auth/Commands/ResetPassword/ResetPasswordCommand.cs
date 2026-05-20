using MediatR;
using Shared.Results;

namespace Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string Token,
    string Password,
    string ConfirmPassword) : IRequest<Result>;

using FluentValidation;

namespace Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .Must(t => !string.IsNullOrWhiteSpace(t))
            .WithMessage("O refresh token é obrigatório.")
            .MaximumLength(8192);
    }
}

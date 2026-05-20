using FluentValidation;

namespace Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .Must(t => !string.IsNullOrWhiteSpace(t))
            .WithMessage("O refresh token é obrigatório.")
            .MaximumLength(8192);
    }
}

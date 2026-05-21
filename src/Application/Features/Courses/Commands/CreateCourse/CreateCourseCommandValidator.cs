using Domain.Courses;
using FluentValidation;

namespace Application.Features.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);

        RuleFor(x => x.PurchaseLink)
            .Must(BeValidHttpUrlOrNull)
            .When(x => !string.IsNullOrWhiteSpace(x.PurchaseLink))
            .WithMessage("Link de compra deve ser uma URL http ou https válida.");

        RuleFor(x => x.ThumbnailUrl)
            .Must(BeValidHttpUrlOrNull)
            .When(x => !string.IsNullOrWhiteSpace(x.ThumbnailUrl))
            .WithMessage("URL da capa deve ser uma URL http ou https válida.");

        RuleFor(x => x.Category)
            .MaximumLength(200)
            .When(x => x.Category is not null);

        RuleFor(x => x.Visibility)
            .IsInEnum();
    }

    private static bool BeValidHttpUrlOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}

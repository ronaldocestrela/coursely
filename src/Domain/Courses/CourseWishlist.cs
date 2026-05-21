namespace Domain.Courses;

/// <summary>
/// Curso desejado na biblioteca do usuário. Regras de negócio ficam aqui, não na API ou no controller.
/// </summary>
public sealed class CourseWishlist
{
    private const int MaxTitleLength = 300;

    private CourseWishlist()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? PurchaseLink { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    public string? Category { get; private set; }

    public CourseVisibility Visibility { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static CourseWishlist Create(
        Guid userId,
        string title,
        string? description,
        string? purchaseLink,
        string? thumbnailUrl,
        string? category,
        CourseVisibility visibility,
        DateTime utcNow)
    {
        ValidateUserId(userId);
        var normalizedTitle = ValidateTitle(title);
        var normalizedPurchase = NormalizePurchaseLink(purchaseLink);
        var normalizedThumb = NormalizeOptionalUrl(thumbnailUrl, nameof(thumbnailUrl));

        return new CourseWishlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = normalizedTitle,
            Description = NormalizeOptional(description),
            PurchaseLink = normalizedPurchase,
            ThumbnailUrl = normalizedThumb,
            Category = NormalizeOptional(category),
            Visibility = visibility,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };
    }

    public void UpdateDetails(
        string title,
        string? description,
        string? purchaseLink,
        string? thumbnailUrl,
        string? category,
        CourseVisibility visibility,
        DateTime utcNow)
    {
        var normalizedTitle = ValidateTitle(title);

        Title = normalizedTitle;
        Description = NormalizeOptional(description);
        PurchaseLink = NormalizePurchaseLink(purchaseLink);
        ThumbnailUrl = NormalizeOptionalUrl(thumbnailUrl, nameof(thumbnailUrl));
        Category = NormalizeOptional(category);
        Visibility = visibility;
        UpdatedAt = utcNow;
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        var trimmed = title.Trim();
        if (trimmed.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalUrl(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (!IsValidHttpUrl(trimmed))
        {
            throw new ArgumentException("URL must be http or https.", paramName);
        }

        return trimmed;
    }

    private static string? NormalizePurchaseLink(string? purchaseLink)
    {
        if (string.IsNullOrWhiteSpace(purchaseLink))
        {
            return null;
        }

        var trimmed = purchaseLink.Trim();
        if (!IsValidHttpUrl(trimmed))
        {
            throw new ArgumentException("Purchase link must be a valid http or https URL when provided.", nameof(purchaseLink));
        }

        return trimmed;
    }

    private static bool IsValidHttpUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}

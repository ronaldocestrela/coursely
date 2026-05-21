using Domain.Courses;
using FluentAssertions;

namespace UnitTests.Courses;

public sealed class CourseWishlistTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly DateTime UtcA = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime UtcB = new(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_throws_when_title_empty(string title)
    {
        var act = () =>
            CourseWishlist.Create(
                UserId,
                title,
                description: null,
                purchaseLink: null,
                thumbnailUrl: null,
                category: null,
                CourseVisibility.Private,
                UtcA);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Create_throws_when_userId_empty()
    {
        var act = () =>
            CourseWishlist.Create(
                Guid.Empty,
                "Curso",
                description: null,
                purchaseLink: null,
                thumbnailUrl: null,
                category: null,
                CourseVisibility.Private,
                UtcA);

        act.Should().Throw<ArgumentException>().WithParameterName("userId");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/x")]
    [InlineData("/relative")]
    public void Create_throws_when_purchase_link_invalid(string? link)
    {
        var act = () =>
            CourseWishlist.Create(
                UserId,
                "Curso",
                description: null,
                purchaseLink: link,
                thumbnailUrl: null,
                category: null,
                CourseVisibility.Private,
                UtcA);

        act.Should().Throw<ArgumentException>().WithParameterName("purchaseLink");
    }

    [Fact]
    public void Create_succeeds_when_purchase_link_https()
    {
        var course = CourseWishlist.Create(
            UserId,
            "Curso",
            description: null,
            purchaseLink: "https://example.com/buy",
            thumbnailUrl: null,
            category: "Dev",
            CourseVisibility.Public,
            UtcA);

        course.PurchaseLink.Should().Be("https://example.com/buy");
        course.Category.Should().Be("Dev");
        course.Visibility.Should().Be(CourseVisibility.Public);
    }

    [Fact]
    public void Create_sets_CreatedAt_and_UpdatedAt_to_same_instant()
    {
        var course = CourseWishlist.Create(
            UserId,
            "Curso",
            description: "Desc",
            purchaseLink: null,
            thumbnailUrl: null,
            category: null,
            CourseVisibility.Shared,
            UtcA);

        course.CreatedAt.Should().Be(UtcA);
        course.UpdatedAt.Should().Be(UtcA);
    }

    [Fact]
    public void UpdateDetails_updates_UpdatedAt()
    {
        var course = CourseWishlist.Create(
            UserId,
            "Curso",
            description: null,
            purchaseLink: null,
            thumbnailUrl: null,
            category: null,
            CourseVisibility.Private,
            UtcA);

        course.UpdateDetails(
            title: "Novo título",
            description: "Nova desc",
            purchaseLink: "http://example.com/p",
            thumbnailUrl: null,
            category: "UX",
            visibility: CourseVisibility.Public,
            UtcB);

        course.Title.Should().Be("Novo título");
        course.UpdatedAt.Should().Be(UtcB);
        course.Visibility.Should().Be(CourseVisibility.Public);
    }
}

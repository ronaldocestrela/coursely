using Application.Features.Courses.Commands.CreateCourse;
using Domain.Courses;
using FluentAssertions;
using FluentValidation;

namespace UnitTests;

public sealed class CreateCourseCommandValidatorTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly CreateCourseCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_fail_when_title_empty(string title)
    {
        var command = new CreateCourseCommand(
            UserId,
            title,
            null,
            null,
            null,
            null,
            CourseVisibility.Private);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCourseCommand.Title));
    }

    [Fact]
    public async Task Should_fail_when_purchase_link_invalid()
    {
        var command = new CreateCourseCommand(
            UserId,
            "Curso",
            null,
            "not-a-url",
            null,
            null,
            CourseVisibility.Private);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCourseCommand.PurchaseLink));
    }

    [Fact]
    public async Task Should_fail_when_user_id_empty()
    {
        var command = new CreateCourseCommand(
            Guid.Empty,
            "Curso",
            null,
            null,
            null,
            null,
            CourseVisibility.Private);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCourseCommand.UserId));
    }

    [Fact]
    public async Task Should_pass_when_valid()
    {
        var command = new CreateCourseCommand(
            UserId,
            "Meu curso",
            "Descrição",
            "https://example.com/buy",
            null,
            "Dev",
            CourseVisibility.Public);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}

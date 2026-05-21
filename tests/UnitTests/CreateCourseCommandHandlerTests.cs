using Application.Features.Courses.Commands.CreateCourse;
using Domain.Courses;
using FluentAssertions;
using Moq;
using Shared.Results;

namespace UnitTests;

public sealed class CreateCourseCommandHandlerTests
{
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    [Fact]
    public async Task Should_create_course_and_persist()
    {
        var fixedInstant = new DateTimeOffset(2026, 3, 1, 15, 0, 0, TimeSpan.Zero);
        var time = new FixedTimeProvider(fixedInstant);

        CourseWishlist? captured = null;
        var mockCreator = new Mock<ICourseCreator>();
        mockCreator
            .Setup(c => c.AddAsync(It.IsAny<CourseWishlist>(), It.IsAny<CancellationToken>()))
            .Callback<CourseWishlist, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);

        var handler = new CreateCourseCommandHandler(mockCreator.Object, time);
        var command = new CreateCourseCommand(
            UserId,
            "Título",
            "Desc",
            "https://buy.example.com",
            null,
            "Cat",
            CourseVisibility.Shared);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Título");
        result.Value.UserId.Should().Be(UserId);
        result.Value.Visibility.Should().Be(CourseVisibility.Shared);
        result.Value.CreatedAt.Should().Be(fixedInstant.UtcDateTime);

        captured.Should().NotBeNull();
        mockCreator.Verify(
            c => c.AddAsync(It.IsAny<CourseWishlist>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_return_failure_when_domain_rejects_url()
    {
        var time = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var mockCreator = new Mock<ICourseCreator>();

        var handler = new CreateCourseCommandHandler(mockCreator.Object, time);
        var command = new CreateCourseCommand(
            UserId,
            "Título",
            null,
            "ftp://bad.example",
            null,
            null,
            CourseVisibility.Private);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(Shared.Courses.CourseErrorCodes.InvalidInput);
        mockCreator.Verify(
            c => c.AddAsync(It.IsAny<CourseWishlist>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

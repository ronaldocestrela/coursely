using Domain.Courses;

namespace Application.Features.Courses.Commands.CreateCourse;

public sealed record CreateCourseResponse(
    Guid Id,
    Guid UserId,
    string Title,
    string? Description,
    string? PurchaseLink,
    string? ThumbnailUrl,
    string? Category,
    CourseVisibility Visibility,
    DateTime CreatedAt,
    DateTime UpdatedAt);

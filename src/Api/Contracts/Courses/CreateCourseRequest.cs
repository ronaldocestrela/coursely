using Domain.Courses;

namespace Api.Contracts.Courses;

public sealed record CreateCourseRequest(
    string Title,
    string? Description,
    string? PurchaseLink,
    string? ThumbnailUrl,
    string? Category,
    CourseVisibility Visibility);

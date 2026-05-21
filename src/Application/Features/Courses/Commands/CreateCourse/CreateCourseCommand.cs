using Domain.Courses;
using MediatR;
using Shared.Results;

namespace Application.Features.Courses.Commands.CreateCourse;

public sealed record CreateCourseCommand(
    Guid UserId,
    string Title,
    string? Description,
    string? PurchaseLink,
    string? ThumbnailUrl,
    string? Category,
    CourseVisibility Visibility)
    : IRequest<Result<CreateCourseResponse>>;

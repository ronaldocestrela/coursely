using Domain.Courses;
using MediatR;
using Shared.Courses;
using Shared.Results;

namespace Application.Features.Courses.Commands.CreateCourse;

public sealed class CreateCourseCommandHandler(ICourseCreator courseCreator, TimeProvider time)
    : IRequestHandler<CreateCourseCommand, Result<CreateCourseResponse>>
{
    public async Task<Result<CreateCourseResponse>> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var utcNow = time.GetUtcNow().UtcDateTime;
            var course = CourseWishlist.Create(
                request.UserId,
                request.Title,
                request.Description,
                request.PurchaseLink,
                request.ThumbnailUrl,
                request.Category,
                request.Visibility,
                utcNow);

            await courseCreator.AddAsync(course, cancellationToken).ConfigureAwait(false);

            return Result.Success(
                new CreateCourseResponse(
                    course.Id,
                    course.UserId,
                    course.Title,
                    course.Description,
                    course.PurchaseLink,
                    course.ThumbnailUrl,
                    course.Category,
                    course.Visibility,
                    course.CreatedAt,
                    course.UpdatedAt));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<CreateCourseResponse>(
                new Error(CourseErrorCodes.InvalidInput, ex.Message));
        }
    }
}

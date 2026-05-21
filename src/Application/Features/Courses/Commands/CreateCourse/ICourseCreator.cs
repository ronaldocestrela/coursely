using Domain.Courses;

namespace Application.Features.Courses.Commands.CreateCourse;

public interface ICourseCreator
{
    Task AddAsync(CourseWishlist course, CancellationToken cancellationToken);
}

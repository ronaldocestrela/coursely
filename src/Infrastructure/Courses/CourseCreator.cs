using Application.Features.Courses.Commands.CreateCourse;
using Domain.Courses;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Courses;

public sealed class CourseCreator(ApplicationDbContext db) : ICourseCreator
{
    public async Task AddAsync(CourseWishlist course, CancellationToken cancellationToken)
    {
        await db.CourseWishlists.AddAsync(course, cancellationToken).ConfigureAwait(false);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

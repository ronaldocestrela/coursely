using System.Security.Claims;
using Api.Contracts.Courses;
using Application.Features.Courses.Commands.CreateCourse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a course wishlist entry for the authenticated user.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateCourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCourseRequest body,
        CancellationToken cancellationToken)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idValue) || !Guid.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await mediator
            .Send(
                new CreateCourseCommand(
                    userId,
                    body.Title,
                    body.Description,
                    body.PurchaseLink,
                    body.ThumbnailUrl,
                    body.Category,
                    body.Visibility),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return BadRequest(new { code = result.Error?.Code, message = result.Error?.Message });
        }

        return Created($"/api/courses/{result.Value!.Id}", result.Value);
    }
}

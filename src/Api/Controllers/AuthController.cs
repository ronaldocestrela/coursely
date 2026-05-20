using Api.Contracts.Auth;
using Application.Features.Auth.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new user account. Password rules: at least 8 characters, uppercase, lowercase, digit, and special character.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
                new RegisterUserCommand(body.Name, body.Email, body.Password),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == AuthErrorCodes.DuplicateEmail)
            {
                return Conflict(new { code = result.Error.Code, message = result.Error.Message });
            }

            return BadRequest(new { code = result.Error?.Code, message = result.Error?.Message });
        }

        var location = $"/api/users/{result.Value!.Id}";
        return Created(location, result.Value);
    }
}

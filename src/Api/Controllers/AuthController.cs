using System.Security.Claims;
using Api.Contracts.Auth;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Authenticates a user and returns JWT access token and refresh token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
                new LoginCommand(body.Email, body.Password),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == AuthErrorCodes.InvalidCredentials)
            {
                return Unauthorized(new { code = result.Error.Code, message = result.Error.Message });
            }

            return BadRequest(new { code = result.Error?.Code, message = result.Error?.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the current authenticated user (requires Bearer token).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idValue) || !Guid.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        return Ok(new MeResponse(userId, email, name, roles));
    }
}

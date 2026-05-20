using System.Security.Claims;
using Api.Contracts.Auth;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Refresh;
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
    /// Exchanges the current refresh token for a rotated refresh token and a new JWT access token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
                new RefreshTokenCommand(body.RefreshToken),
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return UnauthorizedRefresh(result.Error?.Code, result.Error?.Message);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Revokes the supplied refresh token (current device session).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LogoutCommand(body.RefreshToken), cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return BadRequest(new { code = result.Error?.Code, message = result.Error?.Message });
        }

        return NoContent();
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

    private static IActionResult UnauthorizedRefresh(string? code, string? message)
    {
        return new UnauthorizedObjectResult(new { code, message });
    }
}

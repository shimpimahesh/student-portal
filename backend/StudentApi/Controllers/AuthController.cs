using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using StudentApi.Application.Features.Auth;

namespace StudentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController(ISender sender, ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new LoginCommand(request.Username, request.Password), cancellationToken);
        if (response is null)
        {
            logger.LogWarning("Authentication failed for username {Username}", request.Username);
        }

        return response is null
            ? Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication failed",
                Detail = "Invalid username or password."
            })
            : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new RefreshCommand(request.RefreshToken), cancellationToken);
        if (response is null)
        {
            logger.LogWarning("Refresh token authentication failed");
        }

        return response is null
            ? Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Refresh failed",
                Detail = "Invalid or expired refresh token."
            })
            : Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        logger.LogInformation("User logout request processed");
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> Me()
    {
        var response = new CurrentUserResponse(
            User.FindFirstValue("oid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.FindFirstValue("preferred_username") ?? User.FindFirstValue(ClaimTypes.Email),
            User.FindFirstValue("name") ?? User.Identity?.Name,
            User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray(),
            User.FindFirstValue("tid"));

        logger.LogInformation("Current user profile requested");
        return Ok(response);
    }
}

public sealed record CurrentUserResponse(
    string? Id,
    string? Email,
    string? DisplayName,
    string[] Roles,
    string? TenantId);

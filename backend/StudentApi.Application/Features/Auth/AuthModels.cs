using MediatR;
using System.ComponentModel.DataAnnotations;

namespace StudentApi.Application.Features.Auth;

public sealed class LoginRequest
{
    [Required]
	public string Username { get; init; } = string.Empty;

	[Required, MinLength(8)]
	public string Password { get; init; } = string.Empty;
}

public sealed class RefreshRequest
{
	[Required]
	public string RefreshToken { get; init; } = string.Empty;
}
public sealed record UserDto(string Id, string Email, string DisplayName, string Role);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, UserDto User);
public sealed record LoginCommand(string Username, string Password) : IRequest<AuthResponse?>;
public sealed record RefreshCommand(string RefreshToken) : IRequest<AuthResponse?>;
public sealed record LogoutCommand(string RefreshToken) : IRequest;
public sealed record GetCurrentUserQuery : IRequest<UserDto>;

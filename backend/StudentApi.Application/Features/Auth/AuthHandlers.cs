using MediatR;
using Microsoft.Extensions.Logging;
using StudentApi.Application.Common;

namespace StudentApi.Application.Features.Auth;

public sealed class LoginHandler(IAuthService authService, ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, AuthResponse?>
{
    public async Task<AuthResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing login request for {Username}", request.Username);
        var response = await authService.LoginAsync(request.Username, request.Password, cancellationToken);
        if (response is null)
        {
            logger.LogWarning("Login failed for {Username}", request.Username);
        }

        return response;
    }
}

public sealed class RefreshHandler(IAuthService authService, ILogger<RefreshHandler> logger) : IRequestHandler<RefreshCommand, AuthResponse?>
{
    public Task<AuthResponse?> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing refresh token request");
        return Task.FromResult(authService.Refresh(request.RefreshToken));
    }
}

public sealed class LogoutHandler(IAuthService authService, ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        authService.Logout(request.RefreshToken);
        logger.LogInformation("Processed logout request");
        return Task.CompletedTask;
    }
}

public sealed class GetCurrentUserHandler(IAuthService authService, ILogger<GetCurrentUserHandler> logger) : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving current user");
        return Task.FromResult(authService.GetCurrentUser());
    }
}

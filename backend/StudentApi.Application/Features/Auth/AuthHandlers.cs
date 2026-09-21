using MediatR;
using StudentApi.Application.Common;

namespace StudentApi.Application.Features.Auth;

public sealed class LoginHandler(IAuthService authService) : IRequestHandler<LoginCommand, AuthResponse?>
{
    public Task<AuthResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
        => authService.LoginAsync(request.Username, request.Password, cancellationToken);
}

public sealed class RefreshHandler(IAuthService authService) : IRequestHandler<RefreshCommand, AuthResponse?>
{
    public Task<AuthResponse?> Handle(RefreshCommand request, CancellationToken cancellationToken)
        => Task.FromResult(authService.Refresh(request.RefreshToken));
}

public sealed class LogoutHandler(IAuthService authService) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        authService.Logout(request.RefreshToken);
        return Task.CompletedTask;
    }
}

public sealed class GetCurrentUserHandler(IAuthService authService) : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        => Task.FromResult(authService.GetCurrentUser());
}

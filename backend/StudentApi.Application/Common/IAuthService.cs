using StudentApi.Application.Features.Auth;

namespace StudentApi.Application.Common;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken);
    AuthResponse? Refresh(string refreshToken);
    void Logout(string refreshToken);
    UserDto GetCurrentUser();
}

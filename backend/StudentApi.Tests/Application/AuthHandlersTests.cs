using StudentApi.Application.Common;
using StudentApi.Application.Features.Auth;

namespace StudentApi.Tests.Application;

public sealed class AuthHandlersTests
{
    [Fact]
    public async Task LoginHandler_delegates_credentials_and_cancellation_token()
    {
        var expected = CreateAuthResponse();
        var service = new FakeAuthService { LoginResult = expected };
        using var cancellationSource = new CancellationTokenSource();

        var result = await new LoginHandler(service)
            .Handle(new LoginCommand("user@example.com", "password"), cancellationSource.Token);

        Assert.Same(expected, result);
        Assert.Equal("user@example.com", service.Username);
        Assert.Equal("password", service.Password);
        Assert.Equal(cancellationSource.Token, service.CancellationToken);
    }

    [Fact]
    public async Task RefreshHandler_returns_service_result()
    {
        var expected = CreateAuthResponse();
        var service = new FakeAuthService { RefreshResult = expected };

        var result = await new RefreshHandler(service)
            .Handle(new RefreshCommand("refresh-token"), CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal("refresh-token", service.RefreshToken);
    }

    [Fact]
    public async Task LoginHandler_returns_null_when_authentication_fails()
    {
        var result = await new LoginHandler(new FakeAuthService())
            .Handle(new LoginCommand("user@example.com", "wrong-password"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshHandler_returns_null_when_refresh_fails()
    {
        var result = await new RefreshHandler(new FakeAuthService())
            .Handle(new RefreshCommand("expired-token"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task LogoutHandler_delegates_refresh_token()
    {
        var service = new FakeAuthService();

        await new LogoutHandler(service)
            .Handle(new LogoutCommand("refresh-token"), CancellationToken.None);

        Assert.Equal("refresh-token", service.LoggedOutToken);
    }

    [Fact]
    public async Task GetCurrentUserHandler_returns_current_user()
    {
        var expected = new UserDto("id", "user@example.com", "User", "User");
        var service = new FakeAuthService { CurrentUser = expected };

        var result = await new GetCurrentUserHandler(service)
            .Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.Equal(expected, result);
    }

    private static AuthResponse CreateAuthResponse()
        => new("access-token", "refresh-token", DateTime.UtcNow,
            new UserDto("id", "user@example.com", "User", "User"));

    private sealed class FakeAuthService : IAuthService
    {
        public AuthResponse? LoginResult { get; init; }
        public AuthResponse? RefreshResult { get; init; }
        public UserDto CurrentUser { get; init; } = new("id", "email", "name", "User");
        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public string? RefreshToken { get; private set; }
        public string? LoggedOutToken { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<AuthResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken)
        {
            Username = username;
            Password = password;
            CancellationToken = cancellationToken;
            return Task.FromResult(LoginResult);
        }

        public AuthResponse? Refresh(string refreshToken)
        {
            RefreshToken = refreshToken;
            return RefreshResult;
        }

        public void Logout(string refreshToken) => LoggedOutToken = refreshToken;

        public UserDto GetCurrentUser() => CurrentUser;
    }
}

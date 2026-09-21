using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using StudentApi.Application.Common;
using StudentApi.Application.Features.Auth;

namespace StudentApi.Infrastructure.Authentication;

public sealed class AuthService(IOptions<AzureAdOptions> azureAdOptions) : IAuthService
{
    private sealed record RefreshTokenEntry(DateTime ExpiresAt, UserDto User);

    private static readonly ConcurrentDictionary<string, RefreshTokenEntry> RefreshTokens = new();
    private readonly AzureAdOptions azureAd = azureAdOptions.Value;

    public async Task<AuthResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        try
        {
            var application = PublicClientApplicationBuilder
                .Create(azureAd.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, azureAd.TenantId)
                .Build();

            var result = await application
                .AcquireTokenByUsernamePassword(new[] { azureAd.Scope }, username, password)
                .ExecuteAsync(cancellationToken);

            var email = result.Account?.Username ?? username;
            var user = new UserDto(
                result.Account?.HomeAccountId.Identifier ?? email,
                email,
                email,
                "User");

            // Microsoft Entra ID creates and signs this access JWT. The API only validates it.
            return new AuthResponse(result.AccessToken, string.Empty, result.ExpiresOn.UtcDateTime, user);
        }
        catch (MsalException)
        {
            return null;
        }
    }

    public AuthResponse? Refresh(string refreshToken)
    {
        return null;
    }

    public void Logout(string refreshToken) => RefreshTokens.TryRemove(HashToken(refreshToken), out _);

    public UserDto GetCurrentUser() => new("unknown", "unknown", "unknown", "User");

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

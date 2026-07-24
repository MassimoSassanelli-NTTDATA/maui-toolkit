using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Entra;
using Ndbs.MauiToolkit.Auth.Results;
using NSubstitute;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Entra.Tests
{
    public class MsalEntraClientTests
    {
        private static OidcOptionsProvider OptionsProvider() => new(new OidcOptions
        {
            Authority = "https://login.microsoftonline.com/tenant/v2.0",
            ClientId = "entra-client",
            RedirectUri = "myapp://callback",
            Scopes = new List<string> { "openid", "profile", "offline_access", "api://app/access_as_user" },
        });

        private static MsalAuthResult SampleResult() => new()
        {
            AccessToken = "access-token",
            IdToken = "id-token",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
            Claims = new[] { new KeyValuePair<string, string>("sub", "user-1") },
        };

        [Fact]
        public async Task LoginAsync_Success_MapsResultToTokenSetAndClaims()
        {
            var msal = Substitute.For<IMsalPublicClient>();
            msal.AcquireTokenInteractiveAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
                .Returns(SampleResult());
            var client = new MsalEntraClient(msal, OptionsProvider());

            var result = await client.LoginAsync();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Tokens);
            Assert.Equal("access-token", result.Tokens!.AccessToken);
            Assert.Equal("id-token", result.Tokens.IdentityToken);
            Assert.Equal(MsalEntraClient.RefreshTokenSentinel, result.Tokens.RefreshToken);
            Assert.Contains(result.Claims, c => c.Key == "sub" && c.Value == "user-1");
        }

        [Fact]
        public async Task LoginAsync_FiltersReservedOidcScopes()
        {
            IReadOnlyList<string>? captured = null;
            var msal = Substitute.For<IMsalPublicClient>();
            msal.AcquireTokenInteractiveAsync(Arg.Do<IReadOnlyList<string>>(s => captured = s), Arg.Any<CancellationToken>())
                .Returns(SampleResult());
            var client = new MsalEntraClient(msal, OptionsProvider());

            await client.LoginAsync();

            Assert.NotNull(captured);
            Assert.DoesNotContain("openid", captured!);
            Assert.DoesNotContain("profile", captured);
            Assert.DoesNotContain("offline_access", captured);
            Assert.Contains("api://app/access_as_user", captured);
        }

        [Fact]
        public async Task LoginAsync_Cancelled_ReturnsCancelled()
        {
            var msal = Substitute.For<IMsalPublicClient>();
            msal.AcquireTokenInteractiveAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
                .Returns<Task<MsalAuthResult>>(_ => throw new MsalAuthenticationException(AuthenticationErrorCode.Cancelled, "cancelled"));
            var client = new MsalEntraClient(msal, OptionsProvider());

            var result = await client.LoginAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthenticationErrorCode.Cancelled, result.ErrorCode);
        }

        [Fact]
        public async Task LoginAsync_UnexpectedError_ReturnsUnexpected()
        {
            var msal = Substitute.For<IMsalPublicClient>();
            msal.AcquireTokenInteractiveAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
                .Returns<Task<MsalAuthResult>>(_ => throw new InvalidOperationException("boom"));
            var client = new MsalEntraClient(msal, OptionsProvider());

            var result = await client.LoginAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthenticationErrorCode.Unexpected, result.ErrorCode);
        }

        [Fact]
        public async Task RefreshTokenAsync_SilentSuccess_MapsTokenSet()
        {
            var msal = Substitute.For<IMsalPublicClient>();
            msal.AcquireTokenSilentAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
                .Returns(SampleResult());
            var client = new MsalEntraClient(msal, OptionsProvider());

            var result = await client.RefreshTokenAsync("ignored");

            Assert.True(result.IsSuccess);
            Assert.Equal("access-token", result.Tokens!.AccessToken);
            Assert.Equal(MsalEntraClient.RefreshTokenSentinel, result.Tokens.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_NoCachedAccount_ReturnsNoRefreshToken()
        {
            var msal = Substitute.For<IMsalPublicClient>();
            msal.AcquireTokenSilentAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
                .Returns((MsalAuthResult?)null);
            var client = new MsalEntraClient(msal, OptionsProvider());

            var result = await client.RefreshTokenAsync("ignored");

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthenticationErrorCode.NoRefreshToken, result.ErrorCode);
        }

        [Fact]
        public async Task LogoutAsync_CallsSignOut()
        {
            var msal = Substitute.For<IMsalPublicClient>();
            var client = new MsalEntraClient(msal, OptionsProvider());

            await client.LogoutAsync("id-token");

            await msal.Received(1).SignOutAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task LogoutAsync_SignOutFails_DoesNotThrow()
        {
            var msal = Substitute.For<IMsalPublicClient>();
            msal.SignOutAsync(Arg.Any<CancellationToken>())
                .Returns<Task>(_ => throw new InvalidOperationException("remote sign-out failed"));
            var client = new MsalEntraClient(msal, OptionsProvider());

            await client.LogoutAsync("id-token");
        }
    }
}

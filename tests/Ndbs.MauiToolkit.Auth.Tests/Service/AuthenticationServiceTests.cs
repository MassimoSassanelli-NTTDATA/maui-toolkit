using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Ndbs.MauiToolkit.Auth.Client;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Messaging;
using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tests.Fakes;
using Ndbs.MauiToolkit.Auth.Tokens;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Service
{
    public class AuthenticationServiceTests
    {
        private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        private static OidcOptions Options(bool rpLogout = false) => new()
        {
            Authority = "https://idp.example.com",
            ClientId = "mobile-app",
            RedirectUri = "myapp://callback",
            EnableRpInitiatedLogout = rpLogout,
        };

        private static OidcTokenSet Tokens(string access, DateTimeOffset expiry, string refresh = "refresh", string id = "")
            => new() { AccessToken = access, RefreshToken = refresh, IdentityToken = id, AccessTokenExpiration = expiry };

        private static OidcLoginResult SuccessfulLogin()
            => OidcLoginResult.Success(
                Tokens("new-access", Now.AddHours(1)),
                new[] { new KeyValuePair<string, string>("sub", "user-1") });

        private sealed class Harness
        {
            public FakeOidcClient OidcClient = new();
            public FakeSecureStorage Storage = new();
            public FakeConnectivity Connectivity = new(isConnected: true);
            public OidcOptionsProvider OptionsProvider;
            public WeakReferenceMessenger Messenger = new();
            public AuthStateRecorder Recorder = new();
            public FixedTimeProvider Time = new(Now);
            public ITokenStore Store;

            public Harness(OidcOptions? options = null)
            {
                OptionsProvider = new OidcOptionsProvider(options ?? Options());
                Store = new SecureStorageTokenStore(Storage, NullLogger<SecureStorageTokenStore>.Instance);
                Messenger.Register(Recorder);
            }

            public AuthenticationService Create()
                => new(OidcClient, Store, OptionsProvider, Connectivity, Messenger, NullLogger<AuthenticationService>.Instance, Time);
        }

        [Fact]
        public async Task LoginAsync_Offline_ReturnsOfflineFailure()
        {
            var harness = new Harness { Connectivity = { IsConnected = false } };
            var service = harness.Create();

            var result = await service.LoginAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthenticationErrorCode.Offline, result.ErrorCode);
            Assert.Equal(0, harness.OidcClient.LoginCount);
        }

        [Fact]
        public async Task LoginAsync_Success_StoresTokensAndPublishesSignedIn()
        {
            var harness = new Harness();
            harness.OidcClient = new FakeOidcClient(loginFactory: SuccessfulLogin);
            var service = harness.Create();

            var result = await service.LoginAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal("user-1", result.UserProfile!.Subject);
            Assert.NotNull(await harness.Store.GetAsync());
            Assert.Equal(new[] { AuthenticationState.SignedIn }, harness.Recorder.States);
        }

        [Fact]
        public async Task LoginAsync_Failure_DoesNotStoreTokensOrPublish()
        {
            var harness = new Harness();
            harness.OidcClient = new FakeOidcClient(
                loginFactory: () => OidcLoginResult.Failure(AuthenticationErrorCode.Cancelled, "cancelled"));
            var service = harness.Create();

            var result = await service.LoginAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal(AuthenticationErrorCode.Cancelled, result.ErrorCode);
            Assert.Null(await harness.Store.GetAsync());
            Assert.Empty(harness.Recorder.States);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ValidToken_ReturnsWithoutRefresh()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("valid", Now.AddHours(1)));
            var service = harness.Create();

            var token = await service.GetAccessTokenAsync();

            Assert.Equal("valid", token);
            Assert.Equal(0, harness.OidcClient.RefreshCount);
        }

        [Fact]
        public async Task GetAccessTokenAsync_NoTokens_ReturnsNull()
        {
            var harness = new Harness();
            var service = harness.Create();

            Assert.Null(await service.GetAccessTokenAsync());
        }

        [Fact]
        public async Task GetAccessTokenAsync_ExpiredToken_RefreshesAndReturnsNewToken()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("expired", Now.AddMinutes(-5)));
            harness.OidcClient = new FakeOidcClient(
                refreshFactory: _ => OidcRefreshResult.Success(Tokens("refreshed", Now.AddHours(1))));
            var service = harness.Create();

            var token = await service.GetAccessTokenAsync();

            Assert.Equal("refreshed", token);
            Assert.Equal(1, harness.OidcClient.RefreshCount);
            Assert.Equal("refreshed", (await harness.Store.GetAsync())!.AccessToken);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ExpiredOffline_ReturnsNull()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("expired", Now.AddMinutes(-5)));
            harness.Connectivity.IsConnected = false;
            var service = harness.Create();

            var token = await service.GetAccessTokenAsync();

            Assert.Null(token);
            Assert.Equal(0, harness.OidcClient.RefreshCount);
        }

        [Fact]
        public async Task GetAccessTokenAsync_NoRefreshToken_ClearsAndPublishesSessionExpired()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("expired", Now.AddMinutes(-5), refresh: ""));
            var service = harness.Create();

            var token = await service.GetAccessTokenAsync();

            Assert.Null(token);
            Assert.Null(await harness.Store.GetAsync());
            Assert.Contains(AuthenticationState.SessionExpired, harness.Recorder.States);
        }

        [Fact]
        public async Task GetAccessTokenAsync_RefreshFails_ClearsAndPublishesSessionExpired()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("expired", Now.AddMinutes(-5)));
            harness.OidcClient = new FakeOidcClient(
                refreshFactory: _ => OidcRefreshResult.Failure(AuthenticationErrorCode.Protocol, "invalid_grant"));
            var service = harness.Create();

            var token = await service.GetAccessTokenAsync();

            Assert.Null(token);
            Assert.Null(await harness.Store.GetAsync());
            Assert.Contains(AuthenticationState.SessionExpired, harness.Recorder.States);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ConcurrentExpiredCalls_RefreshOnlyOnce()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("expired", Now.AddMinutes(-5)));
            harness.OidcClient = new FakeOidcClient(
                refreshFactory: _ => OidcRefreshResult.Success(Tokens("refreshed", Now.AddHours(1))))
            {
                RefreshDelay = TimeSpan.FromMilliseconds(100),
            };
            var service = harness.Create();

            var tokens = await Task.WhenAll(
                Enumerable.Range(0, 10).Select(_ => service.GetAccessTokenAsync()));

            Assert.All(tokens, t => Assert.Equal("refreshed", t));
            Assert.Equal(1, harness.OidcClient.RefreshCount);
        }

        [Fact]
        public async Task LogoutAsync_ClearsStoreAndPublishesSignedOut()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("valid", Now.AddHours(1), id: "id-token"));
            var service = harness.Create();

            await service.LogoutAsync();

            Assert.Null(await harness.Store.GetAsync());
            Assert.Equal(0, harness.OidcClient.LogoutCount);
            Assert.Equal(new[] { AuthenticationState.SignedOut }, harness.Recorder.States);
        }

        [Fact]
        public async Task LogoutAsync_RemoteRequested_CallsProviderLogout()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("valid", Now.AddHours(1), id: "id-token"));
            var service = harness.Create();

            await service.LogoutAsync(remoteLogout: true);

            Assert.Equal(1, harness.OidcClient.LogoutCount);
            Assert.Equal("id-token", harness.OidcClient.LastLogoutIdToken);
        }

        [Fact]
        public async Task LogoutAsync_RpInitiatedConfigured_CallsProviderLogout()
        {
            var harness = new Harness(Options(rpLogout: true));
            await harness.Store.SaveAsync(Tokens("valid", Now.AddHours(1), id: "id-token"));
            var service = harness.Create();

            await service.LogoutAsync();

            Assert.Equal(1, harness.OidcClient.LogoutCount);
        }

        [Fact]
        public async Task IsAuthenticatedAsync_ReflectsStoredTokens()
        {
            var harness = new Harness();
            var service = harness.Create();
            Assert.False(await service.IsAuthenticatedAsync());

            await harness.Store.SaveAsync(Tokens("valid", Now.AddHours(1)));
            Assert.True(await service.IsAuthenticatedAsync());
        }

        [Fact]
        public async Task IsAuthenticatedAsync_ExpiredButRefreshable_ReturnsTrue()
        {
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("expired", Now.AddMinutes(-5)));
            var service = harness.Create();

            Assert.True(await service.IsAuthenticatedAsync());
        }

        [Fact]
        public async Task GetUserProfileAsync_FromStoredIdentityToken_MapsClaims()
        {
            var idToken = CreateIdToken("{\"sub\":\"user-9\",\"email\":\"u@e.x\"}");
            var harness = new Harness();
            await harness.Store.SaveAsync(Tokens("valid", Now.AddHours(1), id: idToken));
            var service = harness.Create();

            var profile = await service.GetUserProfileAsync();

            Assert.NotNull(profile);
            Assert.Equal("user-9", profile!.Subject);
            Assert.Equal("u@e.x", profile.Email);
        }

        [Fact]
        public async Task GetUserProfileAsync_NotSignedIn_ReturnsNull()
        {
            var harness = new Harness();
            var service = harness.Create();

            Assert.Null(await service.GetUserProfileAsync());
        }

        private static string CreateIdToken(string payloadJson)
        {
            static string Encode(string value)
                => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            return $"{Encode("{\"alg\":\"none\"}")}.{Encode(payloadJson)}.";
        }
    }
}

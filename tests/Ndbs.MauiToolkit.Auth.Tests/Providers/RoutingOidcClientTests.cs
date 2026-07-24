using Ndbs.MauiToolkit.Auth.Client;
using Ndbs.MauiToolkit.Auth.Providers;
using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tests.Fakes;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Providers
{
    public class RoutingOidcClientTests
    {
        private static FakeOidcClient ClientReturning(string marker) =>
            new(loginFactory: () => OidcLoginResult.Failure(AuthenticationErrorCode.Unexpected, marker),
                refreshFactory: _ => OidcRefreshResult.Failure(AuthenticationErrorCode.Unexpected, marker));

        [Fact]
        public async Task LoginAsync_SingleProviderNoActive_DelegatesToThatProvider()
        {
            var only = ClientReturning("only");
            var router = new RoutingOidcClient(
                new[] { new IdentityProviderOidcClient("IAS", only) },
                new ActiveIdentityProvider());

            var result = await router.LoginAsync();

            Assert.Equal("only", result.ErrorMessage);
            Assert.Equal(1, only.LoginCount);
        }

        [Fact]
        public async Task LoginAsync_ActiveProviderSet_DelegatesToActiveProvider()
        {
            var ias = ClientReturning("ias");
            var entra = ClientReturning("entra");
            var active = new ActiveIdentityProvider();
            active.SetActiveProvider("AzureEntra");

            var router = new RoutingOidcClient(
                new[]
                {
                    new IdentityProviderOidcClient("IAS", ias),
                    new IdentityProviderOidcClient("AzureEntra", entra),
                },
                active);

            var result = await router.LoginAsync();

            Assert.Equal("entra", result.ErrorMessage);
            Assert.Equal(1, entra.LoginCount);
            Assert.Equal(0, ias.LoginCount);
        }

        [Fact]
        public async Task RefreshTokenAsync_DelegatesToActiveProvider()
        {
            var ias = ClientReturning("ias");
            var entra = ClientReturning("entra");
            var active = new ActiveIdentityProvider();
            active.SetActiveProvider("IAS");

            var router = new RoutingOidcClient(
                new[]
                {
                    new IdentityProviderOidcClient("IAS", ias),
                    new IdentityProviderOidcClient("AzureEntra", entra),
                },
                active);

            var result = await router.RefreshTokenAsync("refresh-token");

            Assert.Equal("ias", result.ErrorMessage);
            Assert.Equal(1, ias.RefreshCount);
            Assert.Equal(0, entra.RefreshCount);
        }

        [Fact]
        public async Task LogoutAsync_DelegatesToActiveProvider()
        {
            var ias = ClientReturning("ias");
            var entra = ClientReturning("entra");
            var active = new ActiveIdentityProvider();
            active.SetActiveProvider("AzureEntra");

            var router = new RoutingOidcClient(
                new[]
                {
                    new IdentityProviderOidcClient("IAS", ias),
                    new IdentityProviderOidcClient("AzureEntra", entra),
                },
                active);

            await router.LogoutAsync("id-token");

            Assert.Equal(1, entra.LogoutCount);
            Assert.Equal(0, ias.LogoutCount);
        }

        [Fact]
        public async Task LoginAsync_ActiveKeyWithoutMatchingProvider_Throws()
        {
            var active = new ActiveIdentityProvider();
            active.SetActiveProvider("Unknown");

            var router = new RoutingOidcClient(
                new[]
                {
                    new IdentityProviderOidcClient("IAS", ClientReturning("ias")),
                    new IdentityProviderOidcClient("AzureEntra", ClientReturning("entra")),
                },
                active);

            await Assert.ThrowsAsync<InvalidOperationException>(() => router.LoginAsync());
        }

        [Fact]
        public async Task LoginAsync_MultipleProvidersNoActive_Throws()
        {
            var router = new RoutingOidcClient(
                new[]
                {
                    new IdentityProviderOidcClient("IAS", ClientReturning("ias")),
                    new IdentityProviderOidcClient("AzureEntra", ClientReturning("entra")),
                },
                new ActiveIdentityProvider());

            await Assert.ThrowsAsync<InvalidOperationException>(() => router.LoginAsync());
        }

        [Fact]
        public async Task LoginAsync_NoProvidersRegistered_Throws()
        {
            var router = new RoutingOidcClient(
                Array.Empty<IdentityProviderOidcClient>(),
                new ActiveIdentityProvider());

            await Assert.ThrowsAsync<InvalidOperationException>(() => router.LoginAsync());
        }
    }
}

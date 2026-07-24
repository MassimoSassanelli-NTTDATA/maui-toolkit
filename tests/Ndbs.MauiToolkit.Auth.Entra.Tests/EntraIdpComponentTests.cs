using System.Text.Json;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Entra;
using Ndbs.MauiToolkit.Auth.Providers;
using NSubstitute;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Entra.Tests
{
    public class EntraIdpComponentTests
    {
        private static OidcOptionsProvider OptionsProvider() => new(new OidcOptions
        {
            Authority = "https://initial.example.com",
            ClientId = "initial",
            RedirectUri = "myapp://callback",
        });

        private static JsonElement Section(string json) => JsonDocument.Parse(json).RootElement.Clone();

        [Fact]
        public void Metadata_DescribesEntraIdentityProvider()
        {
            var component = new EntraIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            Assert.Equal("AzureEntra", component.SectionKey);
            Assert.Equal("IdentityProvider", component.Category);
            Assert.True(component.IsRequired);
            Assert.Equal("AzureEntra", EntraIdpComponent.ProviderKey);
        }

        [Fact]
        public void Validate_ValidConfig_ReturnsValid()
        {
            var component = new EntraIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            var result = component.Validate(Section("{\"ClientId\":\"abc\",\"Authority\":\"https://login.microsoftonline.com/tenant/v2.0\"}"));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_MissingClientId_ReturnsInvalid()
        {
            var component = new EntraIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            var result = component.Validate(Section("{\"Authority\":\"https://login.microsoftonline.com/tenant/v2.0\"}"));

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_NonHttpAuthority_ReturnsInvalid()
        {
            var component = new EntraIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            var result = component.Validate(Section("{\"ClientId\":\"abc\",\"Authority\":\"ftp://login.microsoftonline.com\"}"));

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ApplyAsync_UpdatesOptionsAndActivatesEntraProvider()
        {
            var options = OptionsProvider();
            var active = new ActiveIdentityProvider();
            var component = new EntraIdpComponent(options, Substitute.For<IAuthenticationService>(), active);

            await component.ApplyAsync(Section("{\"ClientId\":\"entra-client\",\"Authority\":\"https://login.microsoftonline.com/tenant/v2.0\"}"));

            Assert.Equal("https://login.microsoftonline.com/tenant/v2.0", options.Current.Authority);
            Assert.Equal("entra-client", options.Current.ClientId);
            Assert.Equal("AzureEntra", active.ActiveProviderKey);
        }

        [Fact]
        public async Task ResetAsync_SignsOut()
        {
            var authService = Substitute.For<IAuthenticationService>();
            var component = new EntraIdpComponent(OptionsProvider(), authService, new ActiveIdentityProvider());

            await component.ResetAsync();

            await authService.Received(1).LogoutAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
    }
}

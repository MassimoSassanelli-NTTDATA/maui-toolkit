using System.Text.Json;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Ias;
using Ndbs.MauiToolkit.Auth.Providers;
using NSubstitute;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Ias.Tests
{
    public class IasIdpComponentTests
    {
        private static OidcOptionsProvider OptionsProvider() => new(new OidcOptions
        {
            Authority = "https://initial.example.com",
            ClientId = "initial",
            RedirectUri = "myapp://callback",
        });

        private static JsonElement Section(string json) => JsonDocument.Parse(json).RootElement.Clone();

        [Fact]
        public void Metadata_DescribesIasIdentityProvider()
        {
            var component = new IasIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            Assert.Equal("IAS", component.SectionKey);
            Assert.Equal("IdentityProvider", component.Category);
            Assert.True(component.IsRequired);
            Assert.Equal("IAS", IasIdpComponent.ProviderKey);
        }

        [Fact]
        public void Validate_ValidConfig_ReturnsValid()
        {
            var component = new IasIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            var result = component.Validate(Section("{\"ClientId\":\"abc\",\"Authority\":\"https://ias.example.com\"}"));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_MissingClientId_ReturnsInvalid()
        {
            var component = new IasIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            var result = component.Validate(Section("{\"Authority\":\"https://ias.example.com\"}"));

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_NonHttpAuthority_ReturnsInvalid()
        {
            var component = new IasIdpComponent(OptionsProvider(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

            var result = component.Validate(Section("{\"ClientId\":\"abc\",\"Authority\":\"ftp://ias.example.com\"}"));

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ApplyAsync_UpdatesOptionsAndActivatesIasProvider()
        {
            var options = OptionsProvider();
            var active = new ActiveIdentityProvider();
            var component = new IasIdpComponent(options, Substitute.For<IAuthenticationService>(), active);

            await component.ApplyAsync(Section("{\"ClientId\":\"new-client\",\"Authority\":\"https://new.example.com\"}"));

            Assert.Equal("https://new.example.com", options.Current.Authority);
            Assert.Equal("new-client", options.Current.ClientId);
            Assert.Equal("IAS", active.ActiveProviderKey);
        }

        [Fact]
        public async Task ResetAsync_SignsOut()
        {
            var authService = Substitute.For<IAuthenticationService>();
            var component = new IasIdpComponent(OptionsProvider(), authService, new ActiveIdentityProvider());

            await component.ResetAsync();

            await authService.Received(1).LogoutAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
    }
}

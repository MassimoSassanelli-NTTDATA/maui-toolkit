using Ndbs.MauiToolkit.Auth.Configuration;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Configuration
{
    public class OidcOptionsProviderTests
    {
        private static OidcOptions Valid() => new()
        {
            Authority = "https://idp.example.com",
            ClientId = "mobile-app",
            RedirectUri = "myapp://callback",
        };

        [Fact]
        public void Constructor_InvalidOptions_Throws()
            => Assert.Throws<OidcConfigurationException>(() => new OidcOptionsProvider(new OidcOptions()));

        [Fact]
        public void Current_ReturnsIndependentSnapshot()
        {
            var provider = new OidcOptionsProvider(Valid());

            var snapshot = provider.Current;
            snapshot.ClientId = "tampered";

            Assert.Equal("mobile-app", provider.Current.ClientId);
        }

        [Fact]
        public void Replace_ValidOptions_UpdatesCurrentAndRaisesEvent()
        {
            var provider = new OidcOptionsProvider(Valid());
            var raised = false;
            provider.OptionsChanged += (_, _) => raised = true;

            var replacement = Valid();
            replacement.Authority = "https://other-idp.example.com";
            provider.Replace(replacement);

            Assert.True(raised);
            Assert.Equal("https://other-idp.example.com", provider.Current.Authority);
        }

        [Fact]
        public void Update_PartialChange_AppliesAndValidates()
        {
            var provider = new OidcOptionsProvider(Valid());

            provider.Update(o => o.ClientId = "new-client");

            Assert.Equal("new-client", provider.Current.ClientId);
        }

        [Fact]
        public void Update_ResultingInvalidOptions_ThrowsAndKeepsPreviousValue()
        {
            var provider = new OidcOptionsProvider(Valid());

            Assert.Throws<OidcConfigurationException>(() => provider.Update(o => o.ClientId = ""));
            Assert.Equal("mobile-app", provider.Current.ClientId);
        }
    }
}

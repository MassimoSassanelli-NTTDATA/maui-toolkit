using Ndbs.MauiToolkit.Auth.Configuration;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Configuration
{
    public class OidcOptionsValidatorTests
    {
        private static OidcOptions Valid() => new()
        {
            Authority = "https://idp.example.com",
            ClientId = "mobile-app",
            RedirectUri = "myapp://callback",
        };

        [Fact]
        public void Validate_ValidOptions_DoesNotThrow()
        {
            var exception = Record.Exception(() => OidcOptionsValidator.Validate(Valid()));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_MissingAuthority_Throws(string authority)
        {
            var options = Valid();
            options.Authority = authority;

            var ex = Assert.Throws<OidcConfigurationException>(() => OidcOptionsValidator.Validate(options));
            Assert.Contains("Authority", ex.Message);
        }

        [Fact]
        public void Validate_NonHttpAuthority_Throws()
        {
            var options = Valid();
            options.Authority = "ftp://idp.example.com";

            Assert.Throws<OidcConfigurationException>(() => OidcOptionsValidator.Validate(options));
        }

        [Fact]
        public void Validate_MissingClientId_Throws()
        {
            var options = Valid();
            options.ClientId = "";

            var ex = Assert.Throws<OidcConfigurationException>(() => OidcOptionsValidator.Validate(options));
            Assert.Contains("ClientId", ex.Message);
        }

        [Fact]
        public void Validate_MissingRedirectUri_Throws()
        {
            var options = Valid();
            options.RedirectUri = "";

            var ex = Assert.Throws<OidcConfigurationException>(() => OidcOptionsValidator.Validate(options));
            Assert.Contains("RedirectUri", ex.Message);
        }

        [Fact]
        public void Validate_MissingOpenIdScope_Throws()
        {
            var options = Valid();
            options.Scopes = new List<string> { "profile" };

            var ex = Assert.Throws<OidcConfigurationException>(() => OidcOptionsValidator.Validate(options));
            Assert.Contains("openid", ex.Message);
        }
    }
}

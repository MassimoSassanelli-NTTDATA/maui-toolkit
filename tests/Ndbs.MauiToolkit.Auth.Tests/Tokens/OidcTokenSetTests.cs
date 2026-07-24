using Ndbs.MauiToolkit.Auth.Tokens;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Tokens
{
    public class OidcTokenSetTests
    {
        private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public void IsAccessTokenValid_FutureExpiry_ReturnsTrue()
        {
            var tokens = new OidcTokenSet { AccessToken = "a", AccessTokenExpiration = Now.AddMinutes(10) };

            Assert.True(tokens.IsAccessTokenValid(Now));
        }

        [Fact]
        public void IsAccessTokenValid_WithinSafetyMargin_ReturnsFalse()
        {
            var tokens = new OidcTokenSet { AccessToken = "a", AccessTokenExpiration = Now.AddSeconds(30) };

            Assert.False(tokens.IsAccessTokenValid(Now));
        }

        [Fact]
        public void IsAccessTokenValid_NoAccessToken_ReturnsFalse()
        {
            var tokens = new OidcTokenSet { AccessToken = "", AccessTokenExpiration = Now.AddHours(1) };

            Assert.False(tokens.IsAccessTokenValid(Now));
        }

        [Fact]
        public void HasRefreshToken_ReflectsPresence()
        {
            Assert.True(new OidcTokenSet { RefreshToken = "r" }.HasRefreshToken);
            Assert.False(new OidcTokenSet { RefreshToken = "" }.HasRefreshToken);
        }
    }
}

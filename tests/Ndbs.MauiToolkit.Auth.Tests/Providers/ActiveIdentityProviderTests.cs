using Ndbs.MauiToolkit.Auth.Providers;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Providers
{
    public class ActiveIdentityProviderTests
    {
        [Fact]
        public void ActiveProviderKey_Default_IsNull()
        {
            var provider = new ActiveIdentityProvider();

            Assert.Null(provider.ActiveProviderKey);
        }

        [Fact]
        public void SetActiveProvider_UpdatesActiveProviderKey()
        {
            var provider = new ActiveIdentityProvider();

            provider.SetActiveProvider("IAS");

            Assert.Equal("IAS", provider.ActiveProviderKey);
        }

        [Fact]
        public void SetActiveProvider_Again_ReplacesPreviousKey()
        {
            var provider = new ActiveIdentityProvider();

            provider.SetActiveProvider("IAS");
            provider.SetActiveProvider("AzureEntra");

            Assert.Equal("AzureEntra", provider.ActiveProviderKey);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetActiveProvider_NullOrWhitespace_Throws(string? key)
        {
            var provider = new ActiveIdentityProvider();

            Assert.ThrowsAny<ArgumentException>(() => provider.SetActiveProvider(key!));
        }
    }
}

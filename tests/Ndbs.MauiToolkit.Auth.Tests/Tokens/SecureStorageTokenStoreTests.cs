using Microsoft.Extensions.Logging.Abstractions;
using Ndbs.MauiToolkit.Auth.Tests.Fakes;
using Ndbs.MauiToolkit.Auth.Tokens;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Tokens
{
    public class SecureStorageTokenStoreTests
    {
        private static SecureStorageTokenStore CreateStore(FakeSecureStorage storage)
            => new(storage, NullLogger<SecureStorageTokenStore>.Instance);

        [Fact]
        public async Task GetAsync_WhenEmpty_ReturnsNull()
        {
            var store = CreateStore(new FakeSecureStorage());

            Assert.Null(await store.GetAsync());
        }

        [Fact]
        public async Task SaveAsync_ThenGetAsync_RoundTripsTokens()
        {
            var storage = new FakeSecureStorage();
            var store = CreateStore(storage);
            var expiry = DateTimeOffset.UtcNow.AddHours(1);

            await store.SaveAsync(new OidcTokenSet
            {
                AccessToken = "access",
                RefreshToken = "refresh",
                IdentityToken = "id",
                AccessTokenExpiration = expiry,
            });

            var loaded = await store.GetAsync();

            Assert.NotNull(loaded);
            Assert.Equal("access", loaded!.AccessToken);
            Assert.Equal("refresh", loaded.RefreshToken);
            Assert.Equal("id", loaded.IdentityToken);
            Assert.Equal(expiry, loaded.AccessTokenExpiration);
        }

        [Fact]
        public async Task ClearAsync_RemovesStoredTokens()
        {
            var storage = new FakeSecureStorage();
            var store = CreateStore(storage);
            await store.SaveAsync(new OidcTokenSet { AccessToken = "a" });

            await store.ClearAsync();

            Assert.Null(await store.GetAsync());
            Assert.Equal(1, storage.RemoveCount);
        }

        [Fact]
        public async Task GetAsync_WhenStoredValueCorrupt_ReturnsNullAndClears()
        {
            var storage = new FakeSecureStorage();
            await storage.SetAsync(SecureStorageTokenStore.StorageKey, "not-json");
            var store = CreateStore(storage);

            Assert.Null(await store.GetAsync());
            Assert.Equal(1, storage.RemoveCount);
        }
    }
}

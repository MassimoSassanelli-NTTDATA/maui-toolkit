using Ndbs.MauiToolkit.Xia.Startup;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class LastTenantStoreTests
{
    [Fact]
    public async Task GetLastTenant_WhenNothingStored_ReturnsNull()
    {
        var store = new LastTenantStore(new InMemoryKeyValueStore());

        var result = await store.GetLastTenantAsync("user-1");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAndGetLastTenant_RoundTrips()
    {
        var store = new LastTenantStore(new InMemoryKeyValueStore());
        var tenantId = Guid.NewGuid();

        await store.SetLastTenantAsync("user-1", tenantId);

        Assert.Equal(tenantId, await store.GetLastTenantAsync("user-1"));
    }

    [Fact]
    public async Task LastTenant_IsStoredPerUser()
    {
        var store = new LastTenantStore(new InMemoryKeyValueStore());
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await store.SetLastTenantAsync("user-1", tenantA);
        await store.SetLastTenantAsync("user-2", tenantB);

        Assert.Equal(tenantA, await store.GetLastTenantAsync("user-1"));
        Assert.Equal(tenantB, await store.GetLastTenantAsync("user-2"));
    }

    [Fact]
    public void ForceSelectionOnNextLogin_DefaultsToFalse_AndPersists()
    {
        var backing = new InMemoryKeyValueStore();
        var store = new LastTenantStore(backing);

        Assert.False(store.ForceSelectionOnNextLogin);

        store.ForceSelectionOnNextLogin = true;

        // A new instance over the same backing store observes the persisted flag.
        Assert.True(new LastTenantStore(backing).ForceSelectionOnNextLogin);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SetLastTenant_WithoutUserId_Throws(string userId)
    {
        var store = new LastTenantStore(new InMemoryKeyValueStore());

        await Assert.ThrowsAsync<ArgumentException>(() => store.SetLastTenantAsync(userId, Guid.NewGuid()));
    }
}

using NDBS.Xia.Api.Dtos;
using Ndbs.MauiToolkit.Xia.Startup;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class TenantStartupResolverTests
{
    private const string UserId = "user-123";

    private static TenantDto Tenant(string name) => new() { Id = Guid.NewGuid(), Name = name };

    private static TenantStartupResolver CreateResolver(out LastTenantStore store)
    {
        store = new LastTenantStore(new InMemoryKeyValueStore());
        return new TenantStartupResolver(store);
    }

    [Fact]
    public async Task ResolveAsync_NoTenants_ReturnsNoTenants()
    {
        var resolver = CreateResolver(out _);

        var result = await resolver.ResolveAsync(UserId, Array.Empty<TenantDto>());

        Assert.Equal(TenantStartupDecision.NoTenants, result.Decision);
    }

    [Fact]
    public async Task ResolveAsync_SingleTenant_AutoActivatesWithoutSelection()
    {
        var resolver = CreateResolver(out var store);
        store.ForceSelectionOnNextLogin = true; // even a forced selection is ignored for a single tenant
        var only = Tenant("Only");

        var result = await resolver.ResolveAsync(UserId, new[] { only });

        Assert.Equal(TenantStartupDecision.AutoActivate, result.Decision);
        Assert.Same(only, result.Tenant);
    }

    [Fact]
    public async Task ResolveAsync_MultipleTenants_ForceSelection_ShowsSelection()
    {
        var resolver = CreateResolver(out var store);
        var a = Tenant("A");
        var b = Tenant("B");
        await store.SetLastTenantAsync(UserId, a.Id);
        store.ForceSelectionOnNextLogin = true;

        var result = await resolver.ResolveAsync(UserId, new[] { a, b });

        Assert.Equal(TenantStartupDecision.ShowSelection, result.Decision);
    }

    [Fact]
    public async Task ResolveAsync_MultipleTenants_ValidLastTenant_AutoActivates()
    {
        var resolver = CreateResolver(out var store);
        var a = Tenant("A");
        var b = Tenant("B");
        await store.SetLastTenantAsync(UserId, b.Id);

        var result = await resolver.ResolveAsync(UserId, new[] { a, b });

        Assert.Equal(TenantStartupDecision.AutoActivate, result.Decision);
        Assert.Equal(b.Id, result.Tenant!.Id);
    }

    [Fact]
    public async Task ResolveAsync_MultipleTenants_LastTenantNoLongerAllowed_ShowsSelection()
    {
        var resolver = CreateResolver(out var store);
        var a = Tenant("A");
        var b = Tenant("B");
        await store.SetLastTenantAsync(UserId, Guid.NewGuid()); // not in list

        var result = await resolver.ResolveAsync(UserId, new[] { a, b });

        Assert.Equal(TenantStartupDecision.ShowSelection, result.Decision);
    }

    [Fact]
    public async Task ResolveAsync_MultipleTenants_NoLastTenant_ShowsSelection()
    {
        var resolver = CreateResolver(out _);
        var a = Tenant("A");
        var b = Tenant("B");

        var result = await resolver.ResolveAsync(UserId, new[] { a, b });

        Assert.Equal(TenantStartupDecision.ShowSelection, result.Decision);
    }
}

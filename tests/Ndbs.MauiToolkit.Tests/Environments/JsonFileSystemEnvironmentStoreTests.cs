using Ndbs.MauiToolkit.Environments;
using Xunit;

namespace Ndbs.MauiToolkit.Tests.Environments;

public sealed class JsonFileSystemEnvironmentStoreTests : IDisposable
{
    private readonly TempAppDataRootProvider _root = new();
    private readonly InMemoryKeyValueStore _keyValueStore = new();
    private readonly JsonFileSystemEnvironmentStore _store;

    public JsonFileSystemEnvironmentStoreTests()
    {
        _store = new JsonFileSystemEnvironmentStore(_root, _keyValueStore);
    }

    private static SystemEnvironment Env(string name) =>
        EnvironmentTestSupport.ParseSingle($$"""[ { "Name": "{{name}}", "IAS": { "ClientId": "c", "Authority": "https://a" } } ]""");

    [Fact]
    public async Task GetAll_WhenFileMissing_ReturnsEmpty()
    {
        var all = await _store.GetAllAsync();

        Assert.Empty(all);
    }

    [Fact]
    public async Task AddOrUpdate_AddsThenUpdatesByName()
    {
        await _store.AddOrUpdateAsync(Env("Alpha"));
        await _store.AddOrUpdateAsync(Env("Beta"));
        await _store.AddOrUpdateAsync(Env("alpha")); // same name (case-insensitive)

        var all = await _store.GetAllAsync();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task Remove_DeletesEnvironment()
    {
        await _store.AddOrUpdateAsync(Env("Alpha"));

        await _store.RemoveAsync("alpha");

        Assert.Empty(await _store.GetAllAsync());
    }

    [Fact]
    public async Task SetActive_ThenGetActive_ReturnsSelection()
    {
        await _store.AddOrUpdateAsync(Env("Alpha"));

        await _store.SetActiveAsync("user-1", "Alpha");
        var active = await _store.GetActiveAsync("user-1");

        Assert.NotNull(active);
        Assert.Equal("Alpha", active!.Name);
    }

    [Fact]
    public async Task GetActive_WhenSelectionRemoved_ClearsAndReturnsNull()
    {
        await _store.AddOrUpdateAsync(Env("Alpha"));
        await _store.SetActiveAsync("user-1", "Alpha");

        await _store.RemoveAsync("Alpha");
        var active = await _store.GetActiveAsync("user-1");

        Assert.Null(active);
        // The invalid selection must not linger.
        Assert.Null(await _store.GetActiveAsync("user-1"));
    }

    [Fact]
    public async Task ActiveSelection_IsScopedPerUser()
    {
        await _store.AddOrUpdateAsync(Env("Alpha"));
        await _store.AddOrUpdateAsync(Env("Beta"));

        await _store.SetActiveAsync("user-1", "Alpha");
        await _store.SetActiveAsync("user-2", "Beta");

        Assert.Equal("Alpha", (await _store.GetActiveAsync("user-1"))!.Name);
        Assert.Equal("Beta", (await _store.GetActiveAsync("user-2"))!.Name);
    }

    public void Dispose() => _root.Dispose();
}

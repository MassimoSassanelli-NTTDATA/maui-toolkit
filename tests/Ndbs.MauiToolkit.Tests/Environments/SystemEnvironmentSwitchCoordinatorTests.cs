using CommunityToolkit.Mvvm.Messaging;
using Ndbs.MauiToolkit.Context;
using Ndbs.MauiToolkit.Environments;
using Xunit;

namespace Ndbs.MauiToolkit.Tests.Environments;

public sealed class SystemEnvironmentSwitchCoordinatorTests : IDisposable
{
    private readonly TempAppDataRootProvider _root = new();
    private readonly InMemoryKeyValueStore _keyValueStore = new();
    private readonly JsonFileSystemEnvironmentStore _store;
    private readonly UserContext _userContext = new();

    public SystemEnvironmentSwitchCoordinatorTests()
    {
        _store = new JsonFileSystemEnvironmentStore(_root, _keyValueStore);
    }

    private static SystemEnvironment Env(string name, string sections) =>
        EnvironmentTestSupport.ParseSingle($$"""[ { "Name": "{{name}}", {{sections}} } ]""");

    private SystemEnvironmentSwitchCoordinator CreateCoordinator(
        List<string> log,
        params ISystemEnvironmentComponent[] components)
    {
        var validator = new SystemEnvironmentValidator(components);
        return new SystemEnvironmentSwitchCoordinator(
            components,
            validator,
            _store,
            _userContext,
            new WeakReferenceMessenger());
    }

    [Fact]
    public async Task Switch_ResetsInReverseOrder_ThenAppliesPresentSectionsInOrder()
    {
        var log = new List<string>();
        var idp = new FakeComponent("IAS", log, category: "IdentityProvider", required: true, order: 10);
        var api = new FakeComponent("Xia", log, order: 20);
        var local = new FakeComponent(string.Empty, log, order: 90); // section-less: reset only

        var coordinator = CreateCoordinator(log, idp, api, local);
        var target = Env("Alpha", "\"IAS\": {}, \"Xia\": {}");
        await _store.AddOrUpdateAsync(target);

        var result = await coordinator.SwitchAsync(target);

        Assert.True(result.IsSuccess);
        // Reset runs for all components in reverse order first, then apply present sections.
        Assert.Equal(
            new[] { "reset:", "reset:Xia", "reset:IAS", "apply:IAS", "apply:Xia" },
            log);
    }

    [Fact]
    public async Task Switch_SectionlessComponent_IsNeverApplied()
    {
        var log = new List<string>();
        var idp = new FakeComponent("IAS", log, category: "IdentityProvider", required: true, order: 10);
        var local = new FakeComponent(string.Empty, log, order: 90);

        var coordinator = CreateCoordinator(log, idp, local);
        var target = Env("Alpha", "\"IAS\": {}");
        await _store.AddOrUpdateAsync(target);

        await coordinator.SwitchAsync(target);

        Assert.Equal(0, local.ApplyCount);
        Assert.Equal(1, local.ResetCount);
    }

    [Fact]
    public async Task Switch_PersistsActiveSelection()
    {
        var log = new List<string>();
        var idp = new FakeComponent("IAS", log, category: "IdentityProvider", required: true, order: 10);
        var coordinator = CreateCoordinator(log, idp);
        var target = Env("Alpha", "\"IAS\": {}");
        await _store.AddOrUpdateAsync(target);

        await coordinator.SwitchAsync(target);

        var active = await _store.GetActiveAsync(_userContext.UserId);
        Assert.Equal("Alpha", active!.Name);
    }

    [Fact]
    public async Task Switch_InvalidEnvironment_FailsWithoutApplying()
    {
        var log = new List<string>();
        var idp = new FakeComponent("IAS", log, category: "IdentityProvider", required: true, order: 10, valid: false);
        var coordinator = CreateCoordinator(log, idp);
        var target = Env("Alpha", "\"IAS\": {}");
        await _store.AddOrUpdateAsync(target);

        var result = await coordinator.SwitchAsync(target);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, idp.ApplyCount);
        Assert.Empty(log);
    }

    [Fact]
    public async Task ApplyActive_AppliesWithoutResetting()
    {
        var log = new List<string>();
        var idp = new FakeComponent("IAS", log, category: "IdentityProvider", required: true, order: 10);
        var coordinator = CreateCoordinator(log, idp);
        var target = Env("Alpha", "\"IAS\": {}");

        await coordinator.ApplyActiveAsync(target);

        Assert.Equal(1, idp.ApplyCount);
        Assert.Equal(0, idp.ResetCount);
    }

    public void Dispose() => _root.Dispose();
}

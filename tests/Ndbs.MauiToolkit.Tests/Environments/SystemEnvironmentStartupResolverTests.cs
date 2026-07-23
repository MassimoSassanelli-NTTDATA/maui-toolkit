using Ndbs.MauiToolkit.Environments;
using Xunit;

namespace Ndbs.MauiToolkit.Tests.Environments;

public sealed class SystemEnvironmentStartupResolverTests : IDisposable
{
    private readonly TempAppDataRootProvider _root = new();
    private readonly InMemoryKeyValueStore _keyValueStore = new();
    private readonly JsonFileSystemEnvironmentStore _store;
    private readonly SystemEnvironmentStartupResolver _resolver;

    public SystemEnvironmentStartupResolverTests()
    {
        _store = new JsonFileSystemEnvironmentStore(_root, _keyValueStore);
        _resolver = new SystemEnvironmentStartupResolver(_store);
    }

    private static SystemEnvironment Env(string name) =>
        EnvironmentTestSupport.ParseSingle($$"""[ { "Name": "{{name}}", "IAS": {} } ]""");

    [Fact]
    public async Task Resolve_NoEnvironments_ReturnsCapture()
    {
        var result = await _resolver.ResolveAsync(userId: null);

        Assert.Equal(EnvironmentStartupDecision.Capture, result.Decision);
    }

    [Fact]
    public async Task Resolve_EnvironmentsButNoneActive_ReturnsShowSelection()
    {
        await _store.AddOrUpdateAsync(Env("Alpha"));

        var result = await _resolver.ResolveAsync(userId: null);

        Assert.Equal(EnvironmentStartupDecision.ShowSelection, result.Decision);
    }

    [Fact]
    public async Task Resolve_ActiveEnvironment_ReturnsContinue()
    {
        await _store.AddOrUpdateAsync(Env("Alpha"));
        await _store.SetActiveAsync(userId: null, "Alpha");

        var result = await _resolver.ResolveAsync(userId: null);

        Assert.Equal(EnvironmentStartupDecision.Continue, result.Decision);
        Assert.Equal("Alpha", result.ActiveEnvironment!.Name);
    }

    public void Dispose() => _root.Dispose();
}

using Ndbs.MauiToolkit.Environments;
using Xunit;

namespace Ndbs.MauiToolkit.Tests.Environments;

public sealed class SystemEnvironmentProvisioningServiceTests : IDisposable
{
    private readonly TempAppDataRootProvider _root = new();
    private readonly InMemoryKeyValueStore _keyValueStore = new();
    private readonly JsonFileSystemEnvironmentStore _store;

    public SystemEnvironmentProvisioningServiceTests()
    {
        _store = new JsonFileSystemEnvironmentStore(_root, _keyValueStore);
    }

    private SystemEnvironmentProvisioningService CreateService(params ISystemEnvironmentComponent[] components)
    {
        var validator = new SystemEnvironmentValidator(components);
        return new SystemEnvironmentProvisioningService(validator, _store);
    }

    [Fact]
    public void Parse_EmptyPayload_Fails()
    {
        var service = CreateService();

        var result = service.Parse("   ");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Parse_InvalidJson_FailsWithMessage()
    {
        var service = CreateService();

        var result = service.Parse("not json");

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void Parse_DuplicateNames_Fails()
    {
        var service = CreateService(new FakeComponent("IAS", new List<string>(), required: true));
        var payload = """[ { "Name": "A", "IAS": {} }, { "Name": "a", "IAS": {} } ]""";

        var result = service.Parse(payload);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Parse_InvalidComponentSection_Fails()
    {
        var service = CreateService(new FakeComponent("IAS", new List<string>(), required: true, valid: false));

        var result = service.Parse("""[ { "Name": "A", "IAS": {} } ]""");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Parse_MultipleValidationErrors_ReportsAll()
    {
        var service = CreateService(
            new FakeComponent("IAS", new List<string>(), required: true, valid: false),
            new FakeComponent("Xia", new List<string>(), required: true, valid: false));

        var result = service.Parse("""[ { "Name": "A", "IAS": {}, "Xia": {} } ]""");

        Assert.False(result.IsSuccess);
        Assert.Contains("IAS", result.ErrorMessage);
        Assert.Contains("Xia", result.ErrorMessage);
    }

    [Fact]
    public async Task Import_ValidPayload_MergesIntoStore()
    {
        var service = CreateService(new FakeComponent("IAS", new List<string>(), required: true));
        var payload = """[ { "Name": "A", "IAS": {} }, { "Name": "B", "IAS": {} } ]""";

        var result = await service.ImportAsync(payload);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, (await _store.GetAllAsync()).Count);
    }

    public void Dispose() => _root.Dispose();
}

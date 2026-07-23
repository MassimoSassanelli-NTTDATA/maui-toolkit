using System.Text.Json;
using Ndbs.MauiToolkit.Environments;
using Ndbs.MauiToolkit.Workspace;

namespace Ndbs.MauiToolkit.Tests.Environments;

/// <summary>Shared helpers and fakes for the environment tests.</summary>
internal static class EnvironmentTestSupport
{
    public static SystemEnvironment ParseSingle(string json) =>
        SystemEnvironmentJsonSerializer.Deserialize(json)[0];

    public static JsonElement Section(string json) =>
        JsonDocument.Parse(json).RootElement.Clone();
}

/// <summary>An <see cref="IAppDataRootProvider"/> backed by a throwaway temp folder.</summary>
internal sealed class TempAppDataRootProvider : IAppDataRootProvider, IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "env-tests-" + Guid.NewGuid().ToString("N"));

    public TempAppDataRootProvider() => Directory.CreateDirectory(_root);

    public string GetAppDataRoot() => _root;

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best effort cleanup.
        }
    }
}

/// <summary>A configurable fake component that records apply/reset calls into a shared log.</summary>
internal sealed class FakeComponent : ISystemEnvironmentComponent
{
    private readonly List<string> _log;
    private readonly bool _valid;

    public FakeComponent(
        string sectionKey,
        List<string> log,
        string? category = null,
        bool required = false,
        int order = 0,
        bool valid = true)
    {
        SectionKey = sectionKey;
        _log = log;
        Category = category;
        IsRequired = required;
        Order = order;
        _valid = valid;
    }

    public string SectionKey { get; }
    public string? Category { get; }
    public bool IsRequired { get; }
    public int Order { get; }

    public int ApplyCount { get; private set; }
    public int ResetCount { get; private set; }

    public EnvironmentComponentValidation Validate(JsonElement section) =>
        _valid ? EnvironmentComponentValidation.Valid() : EnvironmentComponentValidation.Invalid($"{SectionKey} ungültig");

    public Task ApplyAsync(JsonElement section, CancellationToken cancellationToken = default)
    {
        ApplyCount++;
        _log.Add($"apply:{SectionKey}");
        return Task.CompletedTask;
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        ResetCount++;
        _log.Add($"reset:{SectionKey}");
        return Task.CompletedTask;
    }
}

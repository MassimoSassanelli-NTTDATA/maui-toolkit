using Ndbs.MauiToolkit.Startup;

namespace Ndbs.MauiToolkit.Tests;

/// <summary>
/// An in-memory <see cref="IKeyValueStore"/> used to test environment store and
/// start-up resolver logic without depending on the MAUI <c>Preferences</c> API.
/// </summary>
public sealed class InMemoryKeyValueStore : IKeyValueStore
{
    private readonly Dictionary<string, string> _strings = new();
    private readonly Dictionary<string, bool> _bools = new();

    public string? GetString(string key) => _strings.TryGetValue(key, out var value) ? value : null;

    public void SetString(string key, string value) => _strings[key] = value;

    public void Remove(string key)
    {
        _strings.Remove(key);
        _bools.Remove(key);
    }

    public bool GetBool(string key, bool defaultValue = false) => _bools.TryGetValue(key, out var value) ? value : defaultValue;

    public void SetBool(string key, bool value) => _bools[key] = value;
}

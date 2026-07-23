namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Default implementation of <see cref="IPlatformIconResolver"/> that resolves icon
/// definitions from the collection registered with the DI container.
/// </summary>
public sealed class PlatformIconResolver : IPlatformIconResolver
{
    private readonly IReadOnlyDictionary<string, PlatformIconDefinition> _icons;

    public PlatformIconResolver(IEnumerable<PlatformIconDefinition> icons)
    {
        ArgumentNullException.ThrowIfNull(icons);
        _icons = icons.ToDictionary(x => x.Key);
    }

    /// <inheritdoc/>
    public PlatformIconDefinition? Resolve(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        return _icons.TryGetValue(key, out var icon) ? icon : null;
    }
}

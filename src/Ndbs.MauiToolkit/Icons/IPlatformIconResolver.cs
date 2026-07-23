namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Resolves a <see cref="PlatformIconDefinition"/> by its catalog key at runtime.
/// </summary>
public interface IPlatformIconResolver
{
    /// <summary>
    /// Returns the <see cref="PlatformIconDefinition"/> registered under
    /// <paramref name="key"/>, or <see langword="null"/> when no match is found.
    /// </summary>
    PlatformIconDefinition? Resolve(string? key);
}

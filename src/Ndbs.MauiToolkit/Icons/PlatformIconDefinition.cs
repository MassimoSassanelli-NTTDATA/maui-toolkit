namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Describes a platform-agnostic icon through its cross-platform key,
/// the Material icon name (Android / Windows), and the SF Symbol name (iOS).
/// </summary>
public sealed record PlatformIconDefinition(
    string Key,
    string Material,
    string SFSymbol,
    string? Description = null);

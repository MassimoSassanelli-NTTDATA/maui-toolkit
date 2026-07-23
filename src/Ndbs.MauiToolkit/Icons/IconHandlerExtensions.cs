using Microsoft.Maui.Hosting;

namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Handler registration extensions for <see cref="PlatformIconView"/>.
/// </summary>
public static class IconHandlerExtensions
{
    /// <summary>
    /// Registers the platform-specific <see cref="PlatformIconViewHandler"/> for
    /// <see cref="PlatformIconView"/>.
    /// Call this from <c>builder.ConfigureMauiHandlers</c> in <c>MauiProgram</c>.
    /// </summary>
    public static IMauiHandlersCollection AddNdbsPlatformIconHandlers(
        this IMauiHandlersCollection handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

#if ANDROID || IOS || WINDOWS
        handlers.AddHandler<PlatformIconView, PlatformIconViewHandler>();
#endif

        return handlers;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Ndbs.MauiToolkit.Icons;
using System.Runtime.InteropServices;
using UIKit;

namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// iOS handler for <see cref="PlatformIconView"/>.
/// Renders the icon natively using SF Symbols via <c>UIImage.GetSystemImage</c>.
/// </summary>
public partial class PlatformIconViewHandler
    : ViewHandler<PlatformIconView, UIImageView>
{
    private static readonly PropertyMapper<PlatformIconView, PlatformIconViewHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(PlatformIconView.IconKey)] = MapIcon,
            [nameof(PlatformIconView.Size)]    = MapIcon,
            [nameof(PlatformIconView.Color)]   = MapColor,
        };

    public PlatformIconViewHandler() : base(Mapper) { }

    protected override UIImageView CreatePlatformView()
    {
        return new UIImageView
        {
            ContentMode = UIViewContentMode.ScaleAspectFit,
        };
    }

    protected override void ConnectHandler(UIImageView platformView)
    {
        base.ConnectHandler(platformView);
        MapIcon(this, VirtualView);
        MapColor(this, VirtualView);
    }

    private static void MapIcon(PlatformIconViewHandler handler, PlatformIconView view)
    {
        var resolver = handler.MauiContext?.Services.GetService<IPlatformIconResolver>();
        var icon = resolver?.Resolve(view.IconKey);

        if (icon is null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[PlatformIconView] Icon key not found: '{view.IconKey}'");
            handler.PlatformView.Image = null;
            return;
        }

        var config = UIImageSymbolConfiguration.Create((NFloat)view.Size);
        var image = UIImage.GetSystemImage(icon.SFSymbol, config);

        if (image is null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[PlatformIconView] SF Symbol not found: '{icon.SFSymbol}'");
            handler.PlatformView.Image = null;
            return;
        }

        handler.PlatformView.Image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
    }

    private static void MapColor(PlatformIconViewHandler handler, PlatformIconView view)
    {
        // view.Color can be null during Hot Reload when a binding expression is
        // temporarily incomplete (e.g. {AppThemeBinding Light={StaticResource }}).
        // Fall back to Transparent so the icon silently disappears instead of throwing.
        var color = view.Color ?? Colors.Transparent;

        handler.PlatformView.TintColor = color.ToPlatform();
    }
}

using Android.Widget;
using Android.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Ndbs.MauiToolkit.Icons;

namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Android handler for <see cref="PlatformIconView"/>.
/// Renders the icon as a glyph in a <see cref="TextView"/> using the
/// Material Symbols Outlined font.
/// </summary>
public partial class PlatformIconViewHandler
    : ViewHandler<PlatformIconView, TextView>
{
    private static readonly PropertyMapper<PlatformIconView, PlatformIconViewHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(PlatformIconView.IconKey)] = MapIcon,
            [nameof(PlatformIconView.Size)]    = MapIcon,
            [nameof(PlatformIconView.Color)]   = MapColor,
        };

    public PlatformIconViewHandler() : base(Mapper) { }

    protected override TextView CreatePlatformView()
    {
        var tv = new TextView(Context!);
        tv.Gravity = GravityFlags.Center;
        tv.SetSingleLine(true);
        return tv;
    }

    protected override void ConnectHandler(TextView platformView)
    {
        base.ConnectHandler(platformView);
        ApplyTypeface(platformView);
        MapIcon(this, VirtualView);
        MapColor(this, VirtualView);
    }

    private void ApplyTypeface(TextView platformView)
    {
        var fontManager = MauiContext?.Services.GetService<IFontManager>();
        if (fontManager is null) return;

        var typeface = fontManager.GetTypeface(
            Microsoft.Maui.Font.OfSize(MaterialSymbolsGlyphs.FontFamilyName, 0));

        if (typeface is not null)
            platformView.Typeface = typeface;
        else
            System.Diagnostics.Debug.WriteLine(
                $"[PlatformIconView] Font '{MaterialSymbolsGlyphs.FontFamilyName}' not registered. " +
                "Add the .ttf to Resources/Fonts/ and register it in MauiProgram.ConfigureFonts.");
    }

    private static void MapIcon(PlatformIconViewHandler handler, PlatformIconView view)
    {
        var resolver = handler.MauiContext?.Services.GetService<IPlatformIconResolver>();
        var icon = resolver?.Resolve(view.IconKey);

        if (icon is null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[PlatformIconView] Icon key not found: '{view.IconKey}'");
            handler.PlatformView.Text = string.Empty;
            return;
        }

        var glyph = MaterialSymbolsGlyphs.GetGlyph(icon.Material);
        handler.PlatformView.Text = glyph ?? string.Empty;
        handler.PlatformView.TextSize = (float)view.Size;
    }

    private static void MapColor(PlatformIconViewHandler handler, PlatformIconView view)
    {
        // view.Color can be null during Hot Reload when a binding expression is
        // temporarily incomplete (e.g. {AppThemeBinding Light={StaticResource }}).
        // Fall back to Transparent so the icon silently disappears instead of throwing.
        var color = view.Color ?? Colors.Transparent;

        handler.PlatformView.SetTextColor(color.ToPlatform());
    }
}

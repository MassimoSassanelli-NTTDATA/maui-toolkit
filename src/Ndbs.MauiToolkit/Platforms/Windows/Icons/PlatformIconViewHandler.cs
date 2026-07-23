using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Ndbs.MauiToolkit.Icons;

namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Windows handler for <see cref="PlatformIconView"/>.
/// Renders the icon as a glyph in a <see cref="TextBlock"/> using the
/// Material Symbols Outlined font.
/// </summary>
public partial class PlatformIconViewHandler
    : ViewHandler<PlatformIconView, TextBlock>
{
    private static readonly PropertyMapper<PlatformIconView, PlatformIconViewHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(PlatformIconView.IconKey)] = MapIcon,
            [nameof(PlatformIconView.Size)]    = MapIcon,
            [nameof(PlatformIconView.Color)]   = MapColor,
        };

    public PlatformIconViewHandler() : base(Mapper) { }

    protected override TextBlock CreatePlatformView()
    {
        return new TextBlock
        {
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
            TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
        };
    }

    protected override void ConnectHandler(TextBlock platformView)
    {
        base.ConnectHandler(platformView);
        ApplyFontFamily(platformView);
        MapIcon(this, VirtualView);
        MapColor(this, VirtualView);
    }

    private void ApplyFontFamily(TextBlock platformView)
    {
        var fontManager = MauiContext?.Services.GetService<IFontManager>();
        if (fontManager is null) return;

        var fontFamily = fontManager.GetFontFamily(
            Microsoft.Maui.Font.OfSize(MaterialSymbolsGlyphs.FontFamilyName, 0));

        if (fontFamily is not null)
            platformView.FontFamily = fontFamily;
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
        handler.PlatformView.FontSize = view.Size;
    }

    private static void MapColor(PlatformIconViewHandler handler, PlatformIconView view)
    {
        // view.Color can be null during Hot Reload when a binding expression is
        // temporarily incomplete (e.g. {AppThemeBinding Light={StaticResource }}).
        // Fall back to Transparent so the icon silently disappears instead of throwing.
        var color = view.Color ?? Colors.Transparent;

        handler.PlatformView.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Windows.UI.Color.FromArgb(
                (byte)(color.Alpha * 255),
                (byte)(color.Red   * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue  * 255)));
    }
}

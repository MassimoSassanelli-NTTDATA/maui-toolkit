namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Maps Material icon names to their Unicode glyph code points in the
/// Material Symbols Outlined font (Google Fonts).
/// </summary>
/// <remarks>
/// Add a new entry here whenever a new icon is added to <see cref="AppIconCatalog"/>.
/// The font file <c>MaterialSymbolsOutlined.ttf</c> must be placed under
/// <c>Resources/Fonts/</c> in the consuming app project and registered via
/// <c>ConfigureFonts</c> using the alias <c>MaterialSymbolsOutlined</c>.
/// </remarks>
public static class MaterialSymbolsGlyphs
{
    /// <summary>
    /// The font family alias that must be registered via <c>ConfigureFonts</c>
    /// in the consuming app's <c>MauiProgram</c>.
    /// </summary>
    public const string FontFamilyName = "MaterialSymbolsOutlined";

    private static readonly Dictionary<string, string> Glyphs = new()
    {
        { MaterialIconNames.Sync,        "\uE627" },
        { MaterialIconNames.Download,    "\uF090" },
        { MaterialIconNames.Settings,    "\uE8B8" },
        { MaterialIconNames.Delete,      "\uE872" },
        { MaterialIconNames.ArrowBack,   "\uE5C4" },
        { MaterialIconNames.CheckCircle, "\uE86C" },
        { MaterialIconNames.Warning,     "\uE002" },
        { MaterialIconNames.Info,        "\uE88E" },
        { MaterialIconNames.Add,         "\uE145" },
        { MaterialIconNames.Edit,        "\uE3C9" },
        { MaterialIconNames.Home,        "\uE88A" },
        { MaterialIconNames.Inbox,       "\uE156" },
        { MaterialIconNames.EditNote,    "\uE745" },
        { MaterialIconNames.Send,        "\uE163" },
        { MaterialIconNames.Notifications, "\uE7F4" },
        { MaterialIconNames.Filter,        "\uE152" },
        { MaterialIconNames.Speed,       "\uE9E4" },
        { MaterialIconNames.Checklist,   "\uE6B1" },
        { MaterialIconNames.Inventory,   "\uE1A1" },
        { MaterialIconNames.Search,      "\uE8B6" },
        { MaterialIconNames.Description, "\uE873" },
        { MaterialIconNames.FactCheck,   "\uF0C5" },
        { MaterialIconNames.Signature,   "\uE746" },
        { MaterialIconNames.Documents,   "\uE173" },
        { MaterialIconNames.Photo,       "\uE432" },
        { MaterialIconNames.Close,       "\uE5CD" },
        { MaterialIconNames.Directions,  "\uE52E" },
    };

    /// <summary>
    /// Returns the glyph character for the given Material icon name,
    /// or <see langword="null"/> when no entry is registered.
    /// </summary>
    public static string? GetGlyph(string materialName)
        => Glyphs.TryGetValue(materialName, out var glyph) ? glyph : null;

    /// <summary>
    /// Returns all registered Material icon names.
    /// </summary>
    public static IReadOnlyCollection<string> AllNames => Glyphs.Keys;
}

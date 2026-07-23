namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// Central catalog of every icon used across the application.
/// <para>
/// To add a new icon:
/// <list type="number">
///   <item>Choose names in <see cref="MaterialIconNames"/> and <see cref="SFSymbolNames"/>.</item>
///   <item>Register the glyph code point in <see cref="MaterialSymbolsGlyphs"/>.</item>
///   <item>Add a <see cref="PlatformIconDefinition"/> field below.</item>
///   <item>Add the field to <see cref="All"/>.</item>
/// </list>
/// </para>
/// </summary>
public static class AppIconCatalog
{
    public static readonly PlatformIconDefinition Sync = new(
        nameof(Sync),
        MaterialIconNames.Sync,
        SFSymbolNames.Sync,
        "Synchronization");

    public static readonly PlatformIconDefinition Download = new(
        nameof(Download),
        MaterialIconNames.Download,
        SFSymbolNames.Download,
        "Download");

    public static readonly PlatformIconDefinition Settings = new(
        nameof(Settings),
        MaterialIconNames.Settings,
        SFSymbolNames.Settings,
        "Settings");

    public static readonly PlatformIconDefinition Delete = new(
        nameof(Delete),
        MaterialIconNames.Delete,
        SFSymbolNames.Delete,
        "Delete");

    public static readonly PlatformIconDefinition ArrowBack = new(
        nameof(ArrowBack),
        MaterialIconNames.ArrowBack,
        SFSymbolNames.ArrowBack,
        "Navigate back");

    public static readonly PlatformIconDefinition CheckCircle = new(
        nameof(CheckCircle),
        MaterialIconNames.CheckCircle,
        SFSymbolNames.CheckCircle,
        "Success / confirmed");

    public static readonly PlatformIconDefinition Warning = new(
        nameof(Warning),
        MaterialIconNames.Warning,
        SFSymbolNames.Warning,
        "Warning");

    public static readonly PlatformIconDefinition Info = new(
        nameof(Info),
        MaterialIconNames.Info,
        SFSymbolNames.Info,
        "Information");

    public static readonly PlatformIconDefinition Add = new(
        nameof(Add),
        MaterialIconNames.Add,
        SFSymbolNames.Add,
        "Add / create");

    public static readonly PlatformIconDefinition Edit = new(
        nameof(Edit),
        MaterialIconNames.Edit,
        SFSymbolNames.Edit,
        "Edit");

    public static readonly PlatformIconDefinition Home = new(
        nameof(Home),
        MaterialIconNames.Home,
        SFSymbolNames.Home,
        "Home");

    public static readonly PlatformIconDefinition Inbox = new(
        nameof(Inbox),
        MaterialIconNames.Inbox,
        SFSymbolNames.Inbox,
        "Inbox / Arbeitsvorrat");

    public static readonly PlatformIconDefinition InProgress = new(
        nameof(InProgress),
        MaterialIconNames.EditNote,
        SFSymbolNames.InProgress,
        "In Bearbeitung");

    public static readonly PlatformIconDefinition Send = new(
        nameof(Send),
        MaterialIconNames.Send,
        SFSymbolNames.Send,
        "Versendet");

    public static readonly PlatformIconDefinition Notifications = new(
        nameof(Notifications),
        MaterialIconNames.Notifications,
        SFSymbolNames.Notifications,
        "Erinnerungen");

    public static readonly PlatformIconDefinition Filter = new(
        nameof(Filter),
        MaterialIconNames.Filter,
        SFSymbolNames.Filter,
        "Filter");

    public static readonly PlatformIconDefinition Speed = new(
        nameof(Speed),
        MaterialIconNames.Speed,
        SFSymbolNames.Speed,
        "Betriebszustand");

    public static readonly PlatformIconDefinition Checklist = new(
        nameof(Checklist),
        MaterialIconNames.Checklist,
        SFSymbolNames.Checklist,
        "Vorg�nge");

    public static readonly PlatformIconDefinition Inventory = new(
        nameof(Inventory),
        MaterialIconNames.Inventory,
        SFSymbolNames.Inventory,
        "Komponenten");

    public static readonly PlatformIconDefinition Search = new(
        nameof(Search),
        MaterialIconNames.Search,
        SFSymbolNames.Search,
        "Befundung");

    public static readonly PlatformIconDefinition Description = new(
        nameof(Description),
        MaterialIconNames.Description,
        SFSymbolNames.Description,
        "Beschreibung");

    public static readonly PlatformIconDefinition FactCheck = new(
        nameof(FactCheck),
        MaterialIconNames.FactCheck,
        SFSymbolNames.FactCheck,
        "Auftragsstatus");

    public static readonly PlatformIconDefinition Signature = new(
        nameof(Signature),
        MaterialIconNames.Signature,
        SFSymbolNames.Signature,
        "Unterschrift");

    public static readonly PlatformIconDefinition Documents = new(
        nameof(Documents),
        MaterialIconNames.Documents,
        SFSymbolNames.Documents,
        "Dokumente");

    public static readonly PlatformIconDefinition Photo = new(
        nameof(Photo),
        MaterialIconNames.Photo,
        SFSymbolNames.Photo,
        "Fotos");

    public static readonly PlatformIconDefinition Close = new(
        nameof(Close),
        MaterialIconNames.Close,
        SFSymbolNames.Close,
        "Schlie�en");

    public static readonly PlatformIconDefinition Directions = new(
        nameof(Directions),
        MaterialIconNames.Directions,
        SFSymbolNames.Directions,
        "Navigation / Karte");

    /// <summary>
    /// All registered icon definitions. Used for DI registration and catalog validation.
    /// </summary>
    public static IReadOnlyList<PlatformIconDefinition> All =>
    [
        Sync,
        Download,
        Settings,
        Delete,
        ArrowBack,
        CheckCircle,
        Warning,
        Info,
        Add,
        Edit,
        Home,
        Inbox,
        InProgress,
        Send,
        Notifications,
        Filter,
        Speed,
        Checklist,
        Inventory,
        Search,
        Description,
        FactCheck,
        Signature,
        Documents,
        Photo,
        Close,
        Directions,
    ];
}

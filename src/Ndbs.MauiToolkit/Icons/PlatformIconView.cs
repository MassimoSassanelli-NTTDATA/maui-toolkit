namespace Ndbs.MauiToolkit.Icons;

/// <summary>
/// A platform-aware icon view that renders SF Symbols natively on iOS and
/// Material Symbols (font-based) on Android and Windows.
/// </summary>
/// <remarks>
/// Use via XAML:
/// <code>
/// &lt;icons:PlatformIconView IconKey="Download" Size="24"
///     Color="{AppThemeBinding Light=Black, Dark=White}" /&gt;
/// </code>
/// Register the handler in <c>MauiProgram</c> via
/// <c>builder.ConfigureMauiHandlers(h => h.AddNdbsPlatformIconHandlers())</c>.
/// </remarks>
public class PlatformIconView : View
{
    /// <summary>Identifies the <see cref="IconKey"/> bindable property.</summary>
    public static readonly BindableProperty IconKeyProperty =
        BindableProperty.Create(nameof(IconKey), typeof(string), typeof(PlatformIconView));

    /// <summary>Identifies the <see cref="Size"/> bindable property.</summary>
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(
            nameof(Size),
            typeof(double),
            typeof(PlatformIconView),
            24.0,
            propertyChanged: OnSizeChanged);

    /// <summary>Identifies the <see cref="Color"/> bindable property.</summary>
    public static readonly BindableProperty ColorProperty =
        BindableProperty.Create(nameof(Color), typeof(Color), typeof(PlatformIconView), Colors.Black);

    public PlatformIconView()
    {
        WidthRequest = 24;
        HeightRequest = 24;
    }

    /// <summary>
    /// Gets or sets the key used to look up the icon in <see cref="AppIconCatalog"/>.
    /// </summary>
    public string? IconKey
    {
        get => (string?)GetValue(IconKeyProperty);
        set => SetValue(IconKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon size in platform-independent units.
    /// Also updates <see cref="VisualElement.WidthRequest"/> and
    /// <see cref="VisualElement.HeightRequest"/>.
    /// </summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon tint color.
    /// Use <c>AppThemeBinding</c> for automatic light / dark mode support.
    /// </summary>
    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (PlatformIconView)bindable;
        var size = (double)newValue;
        view.WidthRequest = size;
        view.HeightRequest = size;
    }
}

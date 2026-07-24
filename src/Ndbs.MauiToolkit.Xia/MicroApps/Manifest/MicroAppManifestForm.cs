namespace Ndbs.MauiToolkit.Xia.MicroApps.Manifest
{
    /// <summary>
    /// A single form entry (<c>forms[]</c>) of a micro app manifest (§7.4). The
    /// <see cref="Settings"/> use the fixed key set described in the specification but
    /// are kept as a tolerant string map so undeclared keys do not break parsing.
    /// </summary>
    public sealed class MicroAppManifestForm
    {
        /// <summary>Gets the micro-app-wide unique form name.</summary>
        public required string Name { get; init; }

        /// <summary>Gets the display name, or <see langword="null"/> when absent.</summary>
        public string? DisplayName { get; init; }

        /// <summary>Gets the description, or <see langword="null"/> when absent.</summary>
        public string? Description { get; init; }

        /// <summary>Gets the relative path to the form file (for example <c>./forms/X.xaml</c>).</summary>
        public required string Path { get; init; }

        /// <summary>Gets the relative paths of the completion CSV files.</summary>
        public IReadOnlyList<string> Completions { get; init; } = Array.Empty<string>();

        /// <summary>Gets the relative paths of the text CSV files.</summary>
        public IReadOnlyList<string> Texts { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets the client-side form settings as key/value entries (serialized values),
        /// or an empty map when <c>settings</c> is <see langword="null"/>.
        /// </summary>
        public IReadOnlyDictionary<string, string?> Settings { get; init; }
            = new Dictionary<string, string?>();
    }
}

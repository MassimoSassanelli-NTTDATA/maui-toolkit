namespace Ndbs.MauiToolkit.Xia.MicroApps.Model
{
    /// <summary>
    /// Master data record of a single micro app, stored in <c>microapp.db</c>
    /// (specification §5.1). The <see cref="Name"/> is the unique key.
    /// </summary>
    public sealed class MicroApp
    {
        /// <summary>Gets or sets the tenant-wide unique micro app name (key).</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the display name.</summary>
        public string? Anzeigename { get; set; }

        /// <summary>Gets or sets the description.</summary>
        public string? Beschreibung { get; set; }

        /// <summary>Gets or sets the server-side version tag (ETag) used for change detection.</summary>
        public string? ETag { get; set; }

        /// <summary>Gets or sets the point in time of the last successful content download.</summary>
        public DateTimeOffset? LetzterDownload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the local content is fully in sync
        /// with the server state. A micro app with <see langword="false"/> is treated
        /// as faulty and re-synchronized (§3.2).
        /// </summary>
        public bool IstSynchronisiert { get; set; }

        /// <summary>Gets the customer-specific properties (custom properties).</summary>
        public List<MicroAppEigenschaft> Eigenschaften { get; } = new();

        /// <summary>Gets the forms belonging to this micro app.</summary>
        public List<Formular> Formulare { get; } = new();
    }
}

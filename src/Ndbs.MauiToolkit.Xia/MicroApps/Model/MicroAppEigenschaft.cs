namespace Ndbs.MauiToolkit.Xia.MicroApps.Model
{
    /// <summary>
    /// A customer-specific property (custom property) of a <see cref="MicroApp"/>,
    /// stored as a serialized key/value entry (§5.1).
    /// </summary>
    public sealed class MicroAppEigenschaft
    {
        /// <summary>Gets or sets the surrogate key.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the owning micro app name (foreign key).</summary>
        public string MicroAppName { get; set; } = string.Empty;

        /// <summary>Gets or sets the property key.</summary>
        public string Schluessel { get; set; } = string.Empty;

        /// <summary>Gets or sets the serialized property value (scalar).</summary>
        public string? Wert { get; set; }
    }
}

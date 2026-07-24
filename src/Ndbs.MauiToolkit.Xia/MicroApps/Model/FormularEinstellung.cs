namespace Ndbs.MauiToolkit.Xia.MicroApps.Model
{
    /// <summary>
    /// A single client-side form setting stored as a key/value entry (§5.2).
    /// </summary>
    public sealed class FormularEinstellung
    {
        /// <summary>Gets or sets the surrogate key.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the owning form id (foreign key).</summary>
        public int FormularId { get; set; }

        /// <summary>Gets or sets the setting key.</summary>
        public string Schluessel { get; set; } = string.Empty;

        /// <summary>Gets or sets the serialized setting value.</summary>
        public string? Wert { get; set; }
    }
}

namespace Ndbs.MauiToolkit.Xia.MicroApps.Model
{
    /// <summary>
    /// A completion (Vervollständigung) file reference of a <see cref="Formular"/>
    /// (§5.3). It keeps the file path, name and MIME type as well as the name of the
    /// dynamic content table created from the referenced CSV (§5.4). The content-table
    /// name stays empty until the table has been created.
    /// </summary>
    public sealed class VervollstaendigungsDatei
    {
        /// <summary>Gets or sets the surrogate key.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the owning form id (foreign key).</summary>
        public int FormularId { get; set; }

        /// <summary>Gets or sets the file path relative to the micro app.</summary>
        public string Dateipfad { get; set; } = string.Empty;

        /// <summary>Gets or sets the file name.</summary>
        public string Dateiname { get; set; } = string.Empty;

        /// <summary>Gets or sets the MIME type (derived from the file name).</summary>
        public string? Dateityp { get; set; }

        /// <summary>
        /// Gets or sets the name of the dynamic content table holding the CSV data, or
        /// <see langword="null"/> when no table has been created yet.
        /// </summary>
        public string? Inhaltstabelle { get; set; }
    }
}

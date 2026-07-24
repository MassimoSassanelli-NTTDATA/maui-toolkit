namespace Ndbs.MauiToolkit.Xia.MicroApps.Model
{
    /// <summary>
    /// A form belonging to a <see cref="MicroApp"/> (§5.2). The form file itself is
    /// referenced only by name and type; the completion and text data are held as
    /// separate file references (§5.3).
    /// </summary>
    public sealed class Formular
    {
        /// <summary>Gets or sets the surrogate key.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the owning micro app name (foreign key).</summary>
        public string MicroAppName { get; set; } = string.Empty;

        /// <summary>Gets or sets the micro-app-wide unique form name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the display name.</summary>
        public string? Anzeigename { get; set; }

        /// <summary>Gets or sets the description.</summary>
        public string? Beschreibung { get; set; }

        /// <summary>Gets or sets the file name of the form file (file reference).</summary>
        public string? FormulardateiName { get; set; }

        /// <summary>Gets or sets the MIME type of the form file (derived from its name).</summary>
        public string? FormulardateiTyp { get; set; }

        /// <summary>Gets the form settings as key/value entries.</summary>
        public List<FormularEinstellung> Einstellungen { get; } = new();

        /// <summary>Gets the completion file references of this form.</summary>
        public List<VervollstaendigungsDatei> Vervollstaendigungen { get; } = new();

        /// <summary>Gets the text file references of this form.</summary>
        public List<TextDatei> Texte { get; } = new();
    }
}

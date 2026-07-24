namespace Ndbs.MauiToolkit.Xia.MicroApps.Sync
{
    /// <summary>
    /// Table name prefixes used to distinguish the dynamic content tables created for
    /// completion and text CSV files (§5.4 / §7.3). The prefix is passed to
    /// <c>IDynamicTableImportService.ImportCsvAsync</c> so the two kinds can be told
    /// apart by the generated table name.
    /// </summary>
    public static class MicroAppTablePrefixes
    {
        /// <summary>The prefix for completion (Vervollständigung) content tables.</summary>
        public const string Completion = "compl";

        /// <summary>The prefix for text (Text) content tables.</summary>
        public const string Text = "text";
    }
}

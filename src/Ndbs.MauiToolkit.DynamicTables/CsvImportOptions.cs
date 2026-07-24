namespace Ndbs.MauiToolkit.DynamicTables
{
    public class CsvImportOptions
    {
        public bool InsertInvalidRows { get; set; } = true;
        public bool UseInvalidPlaceholder { get; set; } = false;
        public int BatchSize { get; set; } = 10_000;
        public string TableNamePrefix { get; set; } = "dyn";

        /// <summary>
        /// The field delimiter used when parsing the CSV file. Defaults to ";".
        /// </summary>
        public string Delimiter { get; set; } = ";";

        /// <summary>
        /// The culture used when parsing the CSV file. Defaults to
        /// <see cref="System.Globalization.CultureInfo.InvariantCulture"/>.
        /// </summary>
        public System.Globalization.CultureInfo Culture { get; set; } =
            System.Globalization.CultureInfo.InvariantCulture;
    }
}

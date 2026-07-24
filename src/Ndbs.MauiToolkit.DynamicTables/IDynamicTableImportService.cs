namespace Ndbs.MauiToolkit.DynamicTables
{
    public interface IDynamicTableImportService
    {
        /// <summary>Importiert eine CSV-Datei in eine neu erzeugte dynamische Tabelle.
        /// Gibt den generierten Tabellennamen zurück.</summary>
        Task<CsvImportResult> ImportCsvAsync(string csvFilePath, CancellationToken ct = default);

        /// <summary>Importiert eine CSV-Datei in eine neu erzeugte dynamische Tabelle mit
        /// dem angegebenen Tabellen-Präfix (statt des konfigurierten Standard-Präfixes).
        /// Gibt den generierten Tabellennamen zurück.</summary>
        Task<CsvImportResult> ImportCsvAsync(string csvFilePath, string tableNamePrefix, CancellationToken ct = default);

        /// <summary>Entfernt eine zuvor importierte Tabelle.</summary>
        Task DropTableAsync(string tableName, CancellationToken ct = default);
    }
}

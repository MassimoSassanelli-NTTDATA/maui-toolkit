namespace Ndbs.MauiToolkit.DynamicTables
{
    /// <summary>
    /// Represents the outcome of a CSV import operation.
    /// </summary>
    /// <param name="TableName">The name of the dynamically created table that received the data.</param>
    /// <param name="ImportedRowCount">The number of rows successfully inserted into the table.</param>
    /// <param name="InvalidRecords">The records that contained at least one invalid cell during parsing.</param>
    public sealed record CsvImportResult(
        string TableName,
        int ImportedRowCount,
        IReadOnlyList<Record> InvalidRecords);
}

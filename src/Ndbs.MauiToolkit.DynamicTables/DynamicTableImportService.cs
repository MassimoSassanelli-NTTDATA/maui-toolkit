using Microsoft.Extensions.Logging;

namespace Ndbs.MauiToolkit.DynamicTables
{
    internal class DynamicTableImportService : IDynamicTableImportService
    {
        private readonly IDynamicTableRepository _repo;
        private readonly CsvImportOptions _options;
        private readonly ILogger<DynamicTableImportService> _logger;

        public DynamicTableImportService(
            IDynamicTableRepository repo,
            CsvImportOptions options,
            ILogger<DynamicTableImportService> logger)
        {
            _repo = repo;
            _options = options;
            _logger = logger;
        }

        public async Task<CsvImportResult> ImportCsvAsync(string csvFilePath, CancellationToken ct = default)
            => await ImportCsvAsync(csvFilePath, _options.TableNamePrefix, ct).ConfigureAwait(false);

        public async Task<CsvImportResult> ImportCsvAsync(string csvFilePath, string tableNamePrefix, CancellationToken ct = default)
        {
            var tableName = DynamicTableRepository.CreateNewTableName(tableNamePrefix);
            var invalid = new List<Record>();
            int total = 0;

            using var parser = new CsvParser(csvFilePath, _options.Delimiter, _options.Culture)
            {
                InsertInvalidRows = _options.InsertInvalidRows,
                UseInvalidPlaceholder = _options.UseInvalidPlaceholder
            };
            parser.Load();

            var headers = parser.GetHeaders();
            await _repo.DropTableAsync(tableName);                     // defensiv
            var fields = headers.ToDictionary(h => h, _ => "string");
            await _repo.CreateTableAsync(tableName, fields);

            var records = parser.ReadRecords(_options.BatchSize);
            invalid.AddRange(parser.FailedRecords);
            while (records.Any())
            {
                ct.ThrowIfCancellationRequested();
                var rows = records.Select(r => r.ToDictionary()).ToList();
                await _repo.BulkInsertAsync(tableName, rows);
                total += rows.Count;

                records = parser.ReadRecords(_options.BatchSize);
                invalid.AddRange(parser.FailedRecords);
            }

            if (invalid.Any())
                _logger.LogWarning("CSV import of {File}: {Count} rows had invalid cells (kept anyway).",
                    csvFilePath, invalid.Count);

            return new CsvImportResult(tableName, total, invalid);
        }

        public Task DropTableAsync(string tableName, CancellationToken ct = default)
            => _repo.DropTableAsync(tableName);
    }
}

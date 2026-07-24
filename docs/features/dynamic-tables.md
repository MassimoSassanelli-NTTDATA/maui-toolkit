# Dynamic Tables

## Purpose

Creates SQLite tables at runtime from an unknown-at-compile-time schema and imports
CSV data into them. Lives in the optional, platform-neutral `Ndbs.MauiToolkit.DynamicTables`
assembly and reuses the application's existing Entity Framework Core `DbContext`.

Core types:

- `IDynamicTableRepository` / `DynamicTableRepository`
- `IDynamicTableImportService` / `DynamicTableImportService`
- `IDynamicTableMemoryCache` / `DynamicTableMemoryCache`
- `CsvParser`, `CsvImportOptions`, `CsvImportResult`
- `AddDynamicTables<TContext>()`

## When to Use

Use when an app must ingest tabular data whose columns are not known ahead of time
(for example user-provided CSV files) and store it in the local offline SQLite
database without a compile-time entity model.

Do not use it for domain entities that have a fixed, known schema — model those with
normal EF Core entities instead.

## Assembly and Dependencies

- Separate assembly `Ndbs.MauiToolkit.DynamicTables` (`net10.0`, no MAUI dependency).
- Pulls in `Microsoft.EntityFrameworkCore.Sqlite` and `CsvHelper`, so consumers only
  take these dependencies when they actually need dynamic tables.
- Does not reference `Ndbs.MauiToolkit` or `Ndbs.MauiToolkit.Core`; it is used
  side-by-side with them.

## How to Use

1. Register the services against the app's existing `DbContext`:

   ```csharp
   builder.Services.AddDynamicTables<AppDbContext>(options =>
   {
       options.TableNamePrefix = "import";
       options.Delimiter = ";";
   });
   ```

2. Import a CSV file. A uniquely named table is created and the rows are inserted:

   ```csharp
   CsvImportResult result = await importService.ImportCsvAsync(csvFilePath);
   // result.TableName, result.ImportedRowCount, result.InvalidRecords
   ```

3. Query the imported rows, either as dictionaries or (optionally) cached in memory:

   ```csharp
   var rows = await repository.QueryAsDictionaryAsync(result.TableName);

   await cache.LoadTableAsync(result.TableName);
   var cached = cache.GetRows(result.TableName);
   ```

## Security

- Table and column names are validated against a strict identifier allow-list and
  bracket-quoted; unsupported column types are rejected. This guards against SQL
  injection through identifiers, which cannot be parameterized.
- Row values are always passed as parameters, never string-concatenated into SQL.

## Best Practices

- Reuse the application's registered `DbContext`; do not create a second context or
  connection for dynamic tables.
- Treat `CsvImportResult.InvalidRecords` as a first-class output and surface or log it.
- Drop obsolete dynamic tables (`DropTableAsync`) to avoid unbounded local growth.
- Keep this capability optional: reference the assembly only from apps that import
  dynamic tabular data.

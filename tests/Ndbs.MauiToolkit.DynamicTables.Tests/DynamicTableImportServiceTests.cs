using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ndbs.MauiToolkit.DynamicTables.Tests.Support;
using Xunit;

namespace Ndbs.MauiToolkit.DynamicTables.Tests
{
    public class DynamicTableImportServiceTests
    {
        private static string FixturePath(string fileName)
            => Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

        private static ServiceProvider BuildProvider(SqliteConnection connection, Action<CsvImportOptions>? configure = null)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestDbContext>(o => o.UseSqlite(connection));
            services.AddDynamicTables<TestDbContext>(configure);
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task ImportCsvAsync_ImportsRowsIntoNewTable()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            await using var provider = BuildProvider(connection, o => o.TableNamePrefix = "import");

            using var scope = provider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IDynamicTableImportService>();
            var repo = scope.ServiceProvider.GetRequiredService<IDynamicTableRepository>();

            var result = await importService.ImportCsvAsync(FixturePath("people.csv"));

            Assert.StartsWith("import_", result.TableName);
            Assert.Equal(2, result.ImportedRowCount);
            Assert.Empty(result.InvalidRecords);

            var columns = await repo.GetColumnNamesAsync(result.TableName);
            Assert.Equal(new[] { "Id", "Name", "Score" }, columns);

            var rows = await repo.QueryAsDictionaryAsync(result.TableName);
            Assert.Equal(2, rows.Count);
            Assert.Equal("Alice", rows[0]["Name"]);
        }

        [Fact]
        public async Task ImportCsvAsync_ReportsInvalidRecords()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            await using var provider = BuildProvider(connection);

            using var scope = provider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IDynamicTableImportService>();

            var result = await importService.ImportCsvAsync(FixturePath("people_invalid.csv"));

            // All three rows are imported (InsertInvalidRows defaults to true) and the
            // single invalid row is reported.
            Assert.Equal(3, result.ImportedRowCount);
            Assert.Single(result.InvalidRecords);
        }

        [Fact]
        public async Task DropTableAsync_RemovesImportedTable()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            await using var provider = BuildProvider(connection);

            using var scope = provider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<IDynamicTableImportService>();
            var repo = scope.ServiceProvider.GetRequiredService<IDynamicTableRepository>();

            var result = await importService.ImportCsvAsync(FixturePath("people.csv"));
            await importService.DropTableAsync(result.TableName);

            var columns = await repo.GetColumnNamesAsync(result.TableName);
            Assert.Empty(columns);
        }
    }
}

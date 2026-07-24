using System.Globalization;
using Ndbs.MauiToolkit.DynamicTables.Tests.Support;
using Xunit;

namespace Ndbs.MauiToolkit.DynamicTables.Tests
{
    public class DynamicTableRepositoryTests
    {
        private static Dictionary<string, string> SampleFields() => new()
        {
            ["Id"] = "INTEGER",
            ["Name"] = "TEXT",
            ["Score"] = "REAL",
        };

        [Fact]
        public async Task CreateTable_Insert_And_QueryAsDictionary_RoundTrips()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());
            await repo.AddAsync("Items", new Dictionary<string, object?>
            {
                ["Id"] = 1,
                ["Name"] = "Widget",
                ["Score"] = 3.5,
            });

            var rows = await repo.QueryAsDictionaryAsync("Items");

            var row = Assert.Single(rows);
            Assert.Equal(1L, Convert.ToInt64(row["Id"]));
            Assert.Equal("Widget", row["Name"]);
            Assert.Equal(3.5, Convert.ToDouble(row["Score"], CultureInfo.InvariantCulture));
        }

        [Fact]
        public async Task AddAsync_WithNoData_DoesNotThrowAndInsertsNothing()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());
            await repo.AddAsync("Items", new Dictionary<string, object?>());

            var rows = await repo.QueryAsDictionaryAsync("Items");
            Assert.Empty(rows);
        }

        [Fact]
        public async Task BulkInsert_InsertsAllRows_AcrossBatches()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());

            // More than the internal batch size (200) to exercise batching.
            var rows = Enumerable.Range(1, 450)
                .Select(i => new Dictionary<string, object?>
                {
                    ["Id"] = i,
                    ["Name"] = $"Name{i}",
                    ["Score"] = i + 0.25,
                })
                .ToList();

            await repo.BulkInsertAsync("Items", rows);

            var result = await repo.QueryAsDictionaryAsync("Items");
            Assert.Equal(450, result.Count);
        }

        [Fact]
        public async Task BulkInsert_WithEmptyList_DoesNothing()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());
            await repo.BulkInsertAsync("Items", new List<Dictionary<string, object?>>());

            var result = await repo.QueryAsDictionaryAsync("Items");
            Assert.Empty(result);
        }

        [Fact]
        public async Task Query_Entity_MapsColumnsToProperties()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("People", new Dictionary<string, string>
            {
                ["Id"] = "INTEGER",
                ["Name"] = "TEXT",
            });
            await repo.AddAsync("People", new Dictionary<string, object?> { ["Id"] = 7, ["Name"] = "Alice" });

            var people = repo.Query<TestPerson>("People").ToList();

            var person = Assert.Single(people);
            Assert.Equal(7, person.Id);
            Assert.Equal("Alice", person.Name);
        }

        [Fact]
        public async Task DeleteAsync_RemovesRowById()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());
            await repo.AddAsync("Items", new Dictionary<string, object?> { ["Id"] = 1, ["Name"] = "A" });
            await repo.AddAsync("Items", new Dictionary<string, object?> { ["Id"] = 2, ["Name"] = "B" });

            await repo.DeleteAsync("Items", 1);

            var rows = await repo.QueryAsDictionaryAsync("Items");
            var remaining = Assert.Single(rows);
            Assert.Equal(2L, Convert.ToInt64(remaining["Id"]));
        }

        [Fact]
        public async Task DropTable_RemovesTable()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());
            await repo.DropTableAsync("Items");

            var columns = await repo.GetColumnNamesAsync("Items");
            Assert.Empty(columns);
        }

        [Fact]
        public async Task GetColumnNames_ReturnsColumnsInOrder()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());

            var columns = await repo.GetColumnNamesAsync("Items");

            Assert.Equal(new[] { "Id", "Name", "Score" }, columns);
        }

        [Theory]
        [InlineData("Bad Name")]
        [InlineData("Robert'); DROP TABLE Students;--")]
        [InlineData("Items;DROP")]
        [InlineData("\"quoted\"")]
        [InlineData("1Items")]
        [InlineData("")]
        public async Task CreateTable_WithInvalidTableName_ThrowsArgumentException(string tableName)
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.CreateTableAsync(tableName, SampleFields()));
        }

        [Theory]
        [InlineData("Bad Col")]
        [InlineData("col;DROP")]
        [InlineData("col\"x")]
        public async Task CreateTable_WithInvalidColumnName_ThrowsArgumentException(string columnName)
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            var fields = new Dictionary<string, string> { [columnName] = "TEXT" };

            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.CreateTableAsync("Items", fields));
        }

        [Fact]
        public async Task CreateTable_WithDuplicateColumnNames_ThrowsArgumentException()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            var fields = new Dictionary<string, string>
            {
                ["Name"] = "TEXT",
                ["name"] = "TEXT",
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.CreateTableAsync("Items", fields));
        }

        [Fact]
        public async Task AddAsync_StoresMaliciousValueVerbatim_NoInjection()
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            await repo.CreateTableAsync("Items", SampleFields());
            await repo.CreateTableAsync("Secret", new Dictionary<string, string> { ["Id"] = "INTEGER" });

            const string malicious = "x'); DROP TABLE Secret;--";
            await repo.AddAsync("Items", new Dictionary<string, object?>
            {
                ["Id"] = 1,
                ["Name"] = malicious,
            });

            // The value is stored verbatim (parameterized), not interpreted as SQL.
            var rows = await repo.QueryAsDictionaryAsync("Items");
            Assert.Equal(malicious, Assert.Single(rows)["Name"]);

            // The "Secret" table must still exist - the injection attempt did not drop it.
            var secretColumns = await repo.GetColumnNamesAsync("Secret");
            Assert.NotEmpty(secretColumns);
        }

        [Theory]
        [InlineData("INTEGER")]
        [InlineData("INT")]
        [InlineData("BIGINT")]
        [InlineData("REAL")]
        [InlineData("DOUBLE")]
        [InlineData("FLOAT")]
        [InlineData("BLOB")]
        [InlineData("NUMERIC")]
        [InlineData("DECIMAL")]
        [InlineData("TEXT")]
        [InlineData("STRING")]
        [InlineData("")]
        [InlineData("text")]
        public async Task CreateTable_WithSupportedColumnType_Succeeds(string type)
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            var fields = new Dictionary<string, string> { ["Col"] = type };

            var exception = await Xunit.Record.ExceptionAsync(
                () => repo.CreateTableAsync("Items", fields));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData("VARCHAR(50)")]
        [InlineData("DATETIME")]
        [InlineData("nonsense")]
        public async Task CreateTable_WithUnsupportedColumnType_ThrowsArgumentException(string type)
        {
            using var fixture = new SqliteTestFixture();
            using var context = fixture.CreateContext();
            var repo = new DynamicTableRepository(context);

            var fields = new Dictionary<string, string> { ["Col"] = type };

            await Assert.ThrowsAsync<ArgumentException>(
                () => repo.CreateTableAsync("Items", fields));
        }

        [Fact]
        public async Task Values_RoundTrip_IndependentOfCurrentCulture()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                // de-DE uses a comma as the decimal separator.
                var german = new CultureInfo("de-DE");
                CultureInfo.CurrentCulture = german;
                CultureInfo.CurrentUICulture = german;

                using var fixture = new SqliteTestFixture();
                using var context = fixture.CreateContext();
                var repo = new DynamicTableRepository(context);

                await repo.CreateTableAsync("Numbers", new Dictionary<string, string>
                {
                    ["Id"] = "INTEGER",
                    ["DoubleValue"] = "REAL",
                    ["DecimalValue"] = "NUMERIC",
                });

                await repo.AddAsync("Numbers", new Dictionary<string, object?>
                {
                    ["Id"] = 1,
                    ["DoubleValue"] = 1234.56d,
                    ["DecimalValue"] = 9876.54m,
                });

                var rows = await repo.QueryAsDictionaryAsync("Numbers");
                var row = Assert.Single(rows);

                Assert.Equal(1234.56d, Convert.ToDouble(row["DoubleValue"], CultureInfo.InvariantCulture), 2);
                Assert.Equal(9876.54d, Convert.ToDouble(row["DecimalValue"], CultureInfo.InvariantCulture), 2);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void CreateNewTableName_ProducesValidUniqueName()
        {
            var a = DynamicTableRepository.CreateNewTableName("test");
            var b = DynamicTableRepository.CreateNewTableName("test");

            Assert.StartsWith("test_", a);
            Assert.NotEqual(a, b);
        }

        [Theory]
        [InlineData("bad prefix")]
        [InlineData("pre;fix")]
        [InlineData("")]
        public void CreateNewTableName_WithInvalidPrefix_ThrowsArgumentException(string prefix)
        {
            Assert.Throws<ArgumentException>(() => DynamicTableRepository.CreateNewTableName(prefix));
        }
    }
}

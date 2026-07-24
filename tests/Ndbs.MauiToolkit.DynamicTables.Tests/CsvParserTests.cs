using System.Globalization;
using Xunit;

namespace Ndbs.MauiToolkit.DynamicTables.Tests
{
    public class CsvParserTests
    {
        private static string FixturePath(string fileName)
            => Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

        [Fact]
        public void GetHeaders_BeforeLoad_Throws()
        {
            using var parser = new CsvParser(FixturePath("people.csv"));

            Assert.Throws<InvalidOperationException>(() => parser.GetHeaders());
        }

        [Fact]
        public void ReadRecords_BeforeLoad_Throws()
        {
            using var parser = new CsvParser(FixturePath("people.csv"));

            Assert.Throws<InvalidOperationException>(() => parser.ReadRecords());
        }

        [Fact]
        public void Load_Twice_Throws()
        {
            using var parser = new CsvParser(FixturePath("people.csv"));
            parser.Load();

            Assert.Throws<InvalidOperationException>(() => parser.Load());
        }

        [Fact]
        public void GetHeaders_ReturnsHeaderRow()
        {
            using var parser = new CsvParser(FixturePath("people.csv"));
            parser.Load();

            Assert.Equal(new[] { "Id", "Name", "Score" }, parser.GetHeaders());
        }

        [Fact]
        public void ReadRecords_ValidFile_ReturnsAllRowsWithoutFailures()
        {
            using var parser = new CsvParser(FixturePath("people.csv"));
            parser.Load();

            var records = parser.ReadRecords();

            Assert.Equal(2, records.Count);
            Assert.Empty(parser.FailedRecords);

            var first = records[0].ToDictionary();
            Assert.Equal("1", first["Id"]);
            Assert.Equal("Alice", first["Name"]);
            Assert.Equal("10", first["Score"]);
        }

        [Fact]
        public void ReadRecords_WithCommaDelimiter_ParsesColumns()
        {
            using var parser = new CsvParser(FixturePath("people_comma.csv"), delimiter: ",");
            parser.Load();

            Assert.Equal(new[] { "Id", "Name", "Score" }, parser.GetHeaders());

            var records = parser.ReadRecords();
            Assert.Equal(2, records.Count);
            Assert.Equal("Alice", records[0].ToDictionary()["Name"]);
        }

        [Fact]
        public void ReadRecords_HonorsConfiguredCulture()
        {
            // Passing a specific culture must not throw and must parse normally.
            using var parser = new CsvParser(FixturePath("people.csv"), culture: new CultureInfo("de-DE"));
            parser.Load();

            var records = parser.ReadRecords();
            Assert.Equal(2, records.Count);
        }

        [Fact]
        public void ReadRecords_WithInvalidCell_CollectsFailedRecords()
        {
            using var parser = new CsvParser(FixturePath("people_invalid.csv"));
            parser.Load();

            var records = parser.ReadRecords();

            // Default InsertInvalidRows = true, so all rows are returned.
            Assert.Equal(3, records.Count);

            var failed = Assert.Single(parser.FailedRecords);
            Assert.Contains(failed.Cells, c => c.Column == "Name");
        }

        [Fact]
        public void ReadRecords_WithInsertInvalidRowsFalse_ExcludesInvalidRows()
        {
            using var parser = new CsvParser(FixturePath("people_invalid.csv"))
            {
                InsertInvalidRows = false,
            };
            parser.Load();

            var records = parser.ReadRecords();

            // The single invalid row is excluded; two valid rows remain.
            Assert.Equal(2, records.Count);
            Assert.Single(parser.FailedRecords);
        }

        [Fact]
        public void ReadRecords_WithUseInvalidPlaceholder_ReplacesInvalidValue()
        {
            using var parser = new CsvParser(FixturePath("people_invalid.csv"))
            {
                UseInvalidPlaceholder = true,
            };
            parser.Load();

            var records = parser.ReadRecords();

            Assert.Contains(
                records,
                r => r.Cells.Any(c => c.Value == "[INVALID]"));
        }

        [Fact]
        public void Dispose_ReleasesFileHandle()
        {
            // Copy the fixture to a temp file so we can verify the handle is released
            // by deleting the file after Dispose.
            var tempFile = Path.Combine(Path.GetTempPath(), $"csvparser_{Guid.NewGuid():N}.csv");
            File.Copy(FixturePath("people.csv"), tempFile);

            try
            {
                var parser = new CsvParser(tempFile);
                parser.Load();
                parser.ReadRecords();
                parser.Dispose();

                // If the handle were still open, this would throw IOException on Windows.
                var exception = Xunit.Record.Exception(() => File.Delete(tempFile));
                Assert.Null(exception);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}

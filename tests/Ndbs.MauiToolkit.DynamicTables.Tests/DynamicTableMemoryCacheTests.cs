using Xunit;

namespace Ndbs.MauiToolkit.DynamicTables.Tests
{
    public class DynamicTableMemoryCacheTests
    {
        /// <summary>
        /// A fake repository that records whether it was disposed, used to assert that the
        /// cache does not dispose its injected repository.
        /// </summary>
        private sealed class FakeRepository : IDynamicTableRepository, IDisposable
        {
            private readonly Dictionary<string, List<Dictionary<string, object?>>> _tables;

            public FakeRepository(Dictionary<string, List<Dictionary<string, object?>>> tables)
            {
                _tables = tables;
            }

            public bool Disposed { get; private set; }

            public Task<List<Dictionary<string, object?>>> QueryAsDictionaryAsync(string tableName)
                => Task.FromResult(_tables.TryGetValue(tableName, out var rows)
                    ? rows
                    : new List<Dictionary<string, object?>>());

            public Task CreateTableAsync(string tableName, Dictionary<string, string> fieldNames) => Task.CompletedTask;
            public Task AddAsync(string tableName, Dictionary<string, object?> data) => Task.CompletedTask;
            public Task BulkInsertAsync(string tableName, List<Dictionary<string, object?>> rows) => Task.CompletedTask;
            public IQueryable<T> Query<T>(string tableName) where T : class => new List<T>().AsQueryable();
            public Task DeleteAsync(string tableName, int id) => Task.CompletedTask;
            public Task DropTableAsync(string tableName) => Task.CompletedTask;
            public Task<string[]> GetColumnNamesAsync(string tableName) => Task.FromResult(Array.Empty<string>());

            public void Dispose() => Disposed = true;
        }

        private static FakeRepository CreateRepository()
        {
            var tables = new Dictionary<string, List<Dictionary<string, object?>>>
            {
                ["Items"] = new()
                {
                    new Dictionary<string, object?> { ["Id"] = 1, ["Name"] = "A" },
                    new Dictionary<string, object?> { ["Id"] = 2, ["Name"] = "B" },
                },
                ["Other"] = new()
                {
                    new Dictionary<string, object?> { ["Id"] = 99 },
                },
            };
            return new FakeRepository(tables);
        }

        [Fact]
        public async Task LoadTableAsync_ThenGetRows_ReturnsCachedRows()
        {
            using var cache = new DynamicTableMemoryCache(CreateRepository());

            await cache.LoadTableAsync("Items");
            var rows = cache.GetRows("Items");

            Assert.Equal(2, rows.Count);
            Assert.Equal(1, rows[0]["Id"]);
        }

        [Fact]
        public void GetRows_BeforeLoad_Throws()
        {
            using var cache = new DynamicTableMemoryCache(CreateRepository());

            Assert.Throws<InvalidOperationException>(() => cache.GetRows("Items"));
        }

        [Fact]
        public async Task GetLoadedTableNames_ReturnsLoadedTables()
        {
            using var cache = new DynamicTableMemoryCache(CreateRepository());

            await cache.LoadTableAsync("Items");
            await cache.LoadTableAsync("Other");

            Assert.Equal(new[] { "Items", "Other" }, cache.GetLoadedTableNames().OrderBy(n => n));
        }

        [Fact]
        public async Task ClearTable_RemovesSingleTable()
        {
            using var cache = new DynamicTableMemoryCache(CreateRepository());

            await cache.LoadTableAsync("Items");
            await cache.LoadTableAsync("Other");

            cache.ClearTable("Items");

            Assert.DoesNotContain("Items", cache.GetLoadedTableNames());
            Assert.Contains("Other", cache.GetLoadedTableNames());
            Assert.Throws<InvalidOperationException>(() => cache.GetRows("Items"));
        }

        [Fact]
        public async Task ClearAll_RemovesEverything()
        {
            using var cache = new DynamicTableMemoryCache(CreateRepository());

            await cache.LoadTableAsync("Items");
            await cache.LoadTableAsync("Other");

            cache.ClearAll();

            Assert.Empty(cache.GetLoadedTableNames());
        }

        [Fact]
        public async Task Dispose_DoesNotDisposeInjectedRepository()
        {
            var repository = CreateRepository();
            var cache = new DynamicTableMemoryCache(repository);

            await cache.LoadTableAsync("Items");
            cache.Dispose();

            Assert.False(repository.Disposed);

            // The repository remains usable after the cache has been disposed.
            var rows = await repository.QueryAsDictionaryAsync("Items");
            Assert.Equal(2, rows.Count);
        }
    }
}

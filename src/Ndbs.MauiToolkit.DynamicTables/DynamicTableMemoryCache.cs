using System.Collections.Concurrent;

namespace Ndbs.MauiToolkit.DynamicTables
{
    /// <summary>
    /// Provides in-memory caching and access for dynamic table data.
    /// </summary>
    public class DynamicTableMemoryCache : IDynamicTableMemoryCache, IDisposable
    {
        private readonly IDynamicTableRepository _repository;
        private readonly ConcurrentDictionary<string, List<Dictionary<string, object?>>> _cache = new();
        private readonly HashSet<string> _loadedTables = new(); // Track loaded tables
        private bool _disposed;

        public DynamicTableMemoryCache(IDynamicTableRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Loads all data from the specified table into memory and caches it.
        /// </summary>
        /// <param name="tableName">The name of the table to load.</param>
        public async Task LoadTableAsync(string tableName)
        {
            var data = await _repository.QueryAsDictionaryAsync(tableName);
            _cache[tableName] = data;
            lock (_loadedTables)
            {
                _loadedTables.Add(tableName);
            }
        }

        /// <summary>
        /// Gets all cached rows for the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A list of dictionaries representing the rows, or throws if not loaded.</returns>
        public List<Dictionary<string, object?>> GetRows(string tableName)
        {
            lock (_loadedTables)
            {
                if (!_loadedTables.Contains(tableName))
                    throw new InvalidOperationException($"Table '{tableName}' has not been loaded. Call LoadTableAsync first.");
            }
            return _cache.TryGetValue(tableName, out var rows) ? rows : new List<Dictionary<string, object?>>();
        }

        /// <summary>
        /// Gets the names of all currently loaded tables.
        /// </summary>
        public IEnumerable<string> GetLoadedTableNames()
        {
            lock (_loadedTables)
            {
                // Return a copy to ensure thread safety
                return _loadedTables.ToArray();
            }
        }

        /// <summary>
        /// Clears the cache for the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public void ClearTable(string tableName)
        {
            _cache.TryRemove(tableName, out _);
            lock (_loadedTables)
            {
                _loadedTables.Remove(tableName);
            }
        }

        /// <summary>
        /// Clears the entire in-memory cache.
        /// </summary>
        public void ClearAll()
        {
            _cache.Clear();
            lock (_loadedTables)
            {
                _loadedTables.Clear();
            }
        }

        /// <summary>
        /// Disposes the cache by clearing its in-memory data.
        /// </summary>
        /// <remarks>
        /// The underlying repository is injected and owned by the dependency injection
        /// container, so it is intentionally not disposed here.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
                return;

            // Clear managed resources
            _cache.Clear();
            lock (_loadedTables)
            {
                _loadedTables.Clear();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

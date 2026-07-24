namespace Ndbs.MauiToolkit.DynamicTables
{
    /// <summary>
    /// Defines the contract for in-memory caching and access to dynamic table data.
    /// Provides methods to load, retrieve, and clear table data, as well as to query loaded tables.
    /// </summary>
    public interface IDynamicTableMemoryCache : IDisposable
    {
        /// <summary>
        /// Loads all data from the specified table into memory and caches it.
        /// If the table is already loaded, it will be overwritten with the latest data from the repository.
        /// </summary>
        /// <param name="tableName">The name of the table to load.</param>
        /// <returns>A task representing the asynchronous load operation.</returns>
        Task LoadTableAsync(string tableName);

        /// <summary>
        /// Gets all cached rows for the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>
        /// A list of dictionaries representing the rows of the table.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the table has not been loaded. Call <see cref="LoadTableAsync"/> first.
        /// </exception>
        List<Dictionary<string, object?>> GetRows(string tableName);

        /// <summary>
        /// Clears the cache for the specified table, removing all its data from memory.
        /// </summary>
        /// <param name="tableName">The name of the table to clear.</param>
        void ClearTable(string tableName);

        /// <summary>
        /// Clears the entire in-memory cache, removing all loaded tables and their data.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Gets the names of all currently loaded tables.
        /// </summary>
        /// <returns>
        /// An enumerable of table names that are currently loaded in memory.
        /// </returns>
        IEnumerable<string> GetLoadedTableNames();
    }
}

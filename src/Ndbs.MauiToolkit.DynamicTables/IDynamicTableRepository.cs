namespace Ndbs.MauiToolkit.DynamicTables
{
    /// <summary>
    /// Defines the contract for dynamic table data access and management.
    /// </summary>
    public interface IDynamicTableRepository
    {
        Task CreateTableAsync(string tableName, Dictionary<string, string> fieldNames);
        Task AddAsync(string tableName, Dictionary<string, object?> data);
        Task BulkInsertAsync(string tableName, List<Dictionary<string, object?>> rows);
        IQueryable<T> Query<T>(string tableName) where T : class;
        Task<List<Dictionary<string, object?>>> QueryAsDictionaryAsync(string tableName);
        Task DeleteAsync(string tableName, int id);
        Task DropTableAsync(string tableName);
        Task<string[]> GetColumnNamesAsync(string tableName);
    }
}

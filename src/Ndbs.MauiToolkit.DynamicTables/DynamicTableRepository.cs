using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ndbs.MauiToolkit.DynamicTables
{
	/// <summary>
	/// Provides dynamic table management functionality for a database context.
	/// </summary>
	public class DynamicTableRepository : IDynamicTableRepository
	{
		// Allowed identifier pattern for table and column names (defense against SQL injection
		// through identifiers, which cannot be parameterized in SQL).
		private static readonly Regex IdentifierRegex = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

		private readonly DbContext _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicTableRepository"/> class.
		/// </summary>
		/// <param name="context">The database context to be used for table operations.</param>
		public DynamicTableRepository(DbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Creates a new table with the specified name and fields.
		/// </summary>
		/// <param name="tableName">The name of the table to create.</param>
		/// <param name="fieldNames">A dictionary of field names and their data types.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		public async Task CreateTableAsync(string tableName, Dictionary<string, string> fieldNames)
		{
			var createTableQuery = GenerateCreateTableQuery(tableName, fieldNames);
			await _context.Database.ExecuteSqlRawAsync(createTableQuery);
		}

		/// <summary>
		/// Validates an SQL identifier (table or column name) and returns it bracket-quoted.
		/// </summary>
		/// <param name="identifier">The identifier to validate.</param>
		/// <returns>The bracket-quoted identifier.</returns>
		/// <exception cref="ArgumentException">Thrown if the identifier is invalid.</exception>
		private static string QuoteIdentifier(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier) || !IdentifierRegex.IsMatch(identifier))
				throw new ArgumentException($"Invalid SQL identifier: '{identifier}'.", nameof(identifier));

			return $"[{identifier}]";
		}

		/// <summary>
		/// Inserts a new record into the specified table.
		/// </summary>
		/// <param name="tableName">The name of the table to insert data into.</param>
		/// <param name="data">A dictionary of column names and their corresponding values.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		public async Task AddAsync(string tableName, Dictionary<string, object?> data)
		{
			if (data == null || data.Count == 0)
				return;

			var table = QuoteIdentifier(tableName);
			var columns = data.Keys.ToList();
			var columnList = string.Join(", ", columns.Select(QuoteIdentifier));

			var parameters = new List<SqliteParameter>();
			var placeholders = new List<string>();
			for (int i = 0; i < columns.Count; i++)
			{
				var paramName = $"@p{i}";
				placeholders.Add(paramName);
				parameters.Add(new SqliteParameter(paramName, data[columns[i]] ?? DBNull.Value));
			}

			var sql = $"INSERT INTO {table} ({columnList}) VALUES ({string.Join(", ", placeholders)})";
			await _context.Database.ExecuteSqlRawAsync(sql, parameters);
		}

		public async Task BulkInsertAsync(string tableName, List<Dictionary<string, object?>> rows)
		{
			const int batchSize = 200;
			if (rows == null || rows.Count == 0)
				return;

			var table = QuoteIdentifier(tableName);
			var columns = rows[0].Keys.ToList();
			var columnList = string.Join(", ", columns.Select(QuoteIdentifier));

			using var transaction = await _context.Database.BeginTransactionAsync();

			for (int i = 0; i < rows.Count; i += batchSize)
			{
				var batch = rows.Skip(i).Take(batchSize).ToList();
				var valuesList = new List<string>();
				var parameters = new List<SqliteParameter>();
				int paramIndex = 0;

				foreach (var row in batch)
				{
					var placeholders = new List<string>(columns.Count);
					foreach (var col in columns)
					{
						var paramName = $"@p{paramIndex++}";
						placeholders.Add(paramName);
						parameters.Add(new SqliteParameter(paramName, row[col] ?? DBNull.Value));
					}
					valuesList.Add($"({string.Join(", ", placeholders)})");
				}

				var sql = $"INSERT INTO {table} ({columnList}) VALUES {string.Join(", ", valuesList)}";
				await _context.Database.ExecuteSqlRawAsync(sql, parameters);
			}

			await transaction.CommitAsync();
		}

		/// <summary>
		/// Queries all records from the specified table and maps them to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to map the query results to.</typeparam>
		/// <param name="tableName">The name of the table to query.</param>
		/// <returns>An <see cref="IQueryable{T}"/> representing the query results.</returns>
		public IQueryable<T> Query<T>(string tableName) where T : class
		{
			var query = $"SELECT * FROM {QuoteIdentifier(tableName)}";
			return _context.Set<T>().FromSqlRaw(query);
		}

		/// <summary>
		 /// Queries all records from the specified table and returns each row as a dictionary of column name to value.
		 /// </summary>
		 /// <param name="tableName">The name of the table to query.</param>
		 /// <returns>A list of dictionaries, each representing a row with column names as keys.</returns>
		public async Task<List<Dictionary<string, object?>>> QueryAsDictionaryAsync(string tableName)
		{
			var result = new List<Dictionary<string, object?>>();
			var query = $"SELECT * FROM {QuoteIdentifier(tableName)}";

			var connection = _context.Database.GetDbConnection();
			await using var command = connection.CreateCommand();
			command.CommandText = query;
			await _context.Database.OpenConnectionAsync();

			try
			{
				await using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					var row = new Dictionary<string, object?>();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						var columnName = reader.GetName(i);
						var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
						row[columnName] = value;
					}
					result.Add(row);
				}
			}
			finally
			{
				await _context.Database.CloseConnectionAsync();
			}

			return result;
		}

		/// <summary>
		/// Deletes a record from the specified table by its ID.
		/// </summary>
		/// <param name="tableName">The name of the table to delete the record from.</param>
		/// <param name="id">The ID of the record to delete.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		public async Task DeleteAsync(string tableName, int id)
		{
			var deleteQuery = $"DELETE FROM {QuoteIdentifier(tableName)} WHERE Id = @id";
			await _context.Database.ExecuteSqlRawAsync(deleteQuery, new SqliteParameter("@id", id));
		}

		/// <summary>
		/// Drops the specified table from the database.
		/// </summary>
		/// <param name="tableName">The name of the table to drop.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		public async Task DropTableAsync(string tableName)
		{
			var dropQuery = $"DROP TABLE IF EXISTS {QuoteIdentifier(tableName)}";
			await _context.Database.ExecuteSqlRawAsync(dropQuery);
		}

		/// <summary>
		/// Retrieves the column names of the specified table.
		/// </summary>
		/// <param name="tableName">The name of the table to retrieve column names from.</param>
		/// <returns>An array of column names.</returns>
		public async Task<string[]> GetColumnNamesAsync(string tableName)
		{
			// Validate the identifier; PRAGMA table_info does not support bound parameters,
			// so the (validated) name is embedded as a quoted string literal.
			QuoteIdentifier(tableName);
			var columnNamesQuery = $"PRAGMA table_info('{tableName}')";
			var columnNamesList = new List<string>();

			var connection = _context.Database.GetDbConnection();
			await using var command = connection.CreateCommand();
			command.CommandText = columnNamesQuery;
			await _context.Database.OpenConnectionAsync();

			try
			{
				await using var reader = await command.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					var columnName = reader["name"]?.ToString();
					if (columnName != null)
					{
						columnNamesList.Add(columnName);
					}
				}
			}
			finally
			{
				await _context.Database.CloseConnectionAsync();
			}

			return columnNamesList.ToArray();
		}

		/// <summary>
		/// Generates a SQL query to create a table with the specified fields.
		/// </summary>
		/// <param name="tableName">The name of the table to create.</param>
		/// <param name="fieldNames">A dictionary of field names and their data types.</param>
		/// <returns>A SQL query string to create the table.</returns>
		private string GenerateCreateTableQuery(string tableName, Dictionary<string, string> fieldNames)
		{
			var table = QuoteIdentifier(tableName);
			var fields = new List<string>();
			var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var field in fieldNames)
			{
				if (!seenNames.Add(field.Key))
					throw new ArgumentException($"Duplicate field name detected: '{field.Key}'", nameof(fieldNames));

				// Validate column name; column type is constrained to a known allow list.
				var column = QuoteIdentifier(field.Key);
				var type = NormalizeColumnType(field.Value);
				fields.Add($"{column} {type}");
			}

			var fieldsString = string.Join(", ", fields);
			return $"CREATE TABLE {table} ({fieldsString})"; // No NOT NULL, so columns are nullable
		}

		/// <summary>
		/// Maps a requested column type to a safe, supported SQLite storage type.
		/// </summary>
		private static string NormalizeColumnType(string type)
		{
			return (type?.Trim().ToUpperInvariant()) switch
			{
				"INTEGER" or "INT" or "BIGINT" => "INTEGER",
				"REAL" or "DOUBLE" or "FLOAT" => "REAL",
				"BLOB" => "BLOB",
				"NUMERIC" or "DECIMAL" => "NUMERIC",
				"TEXT" or "STRING" or "" or null => "TEXT",
				_ => throw new ArgumentException($"Unsupported column type: '{type}'.", nameof(type)),
			};
		}

		public static string CreateNewTableName(string prefix = "dyn")
		{
			// Validate the prefix to ensure it is a valid SQLite table name
			if (string.IsNullOrWhiteSpace(prefix) || prefix.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
			{
				throw new ArgumentException("Prefix must be a valid SQLite table name prefix.", nameof(prefix));
			}

			// create a safe sqlite table name using a GUID to ensure uniqueness
			return prefix + "_" + Guid.NewGuid().ToString("N");
		}
	}
}

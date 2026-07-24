using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Ndbs.MauiToolkit.DynamicTables
{
	/// <summary>
	/// Provides functionality to read and parse CSV files with robust error handling at the cell level.
	/// <para>
	/// <b>Usage:</b> Instantiate <see cref="CsvParser"/> with a file path, call <see cref="Load"/>, and then use <see cref="ReadRecords"/> to retrieve records.
	/// The parser supports detection and reporting of invalid cells and allows you to control how invalid data is handled.
	/// </para>
	/// <para>
	/// <b>Features:</b>
	/// <list type="bullet">
	/// <item>Reads CSV files with configurable delimiter and quoting behavior.</item>
	/// <item>Detects and collects information about invalid cells (row, column, value).</item>
	/// <item>Optionally replaces invalid cell values with a placeholder (e.g., "[INVALID]").</item>
	/// <item>Allows you to include or exclude rows with invalid cells from the result set.</item>
	/// <item>Provides access to all failed records for custom logging or error handling.</item>
	/// </list>
	/// </para>
	/// <para>
	/// <b>Important:</b> This class holds file handles and unmanaged resources. Always call <see cref="Dispose"/> (or use a <c>using</c> statement)
	/// to release resources after parsing is complete.
	/// </para>
	/// </summary>
	public class CsvParser : IDisposable
	{
		private CsvReader? _csvReader;
		private TextReader? _reader;
		private bool _loaded = false;
		private bool _handlingBadDataFound = false;

		private readonly CsvConfiguration _configuration;
		private readonly string _filePath;

		/// <summary>
		/// Gets or sets whether invalid cell values should be replaced with the string "[INVALID]".
		/// If set to <c>false</c>, the original (possibly null or malformed) value is retained.
		/// Default is <c>false</c>.
		/// </summary>
		public bool UseInvalidPlaceholder { get; set; } = false;

		/// <summary>
		/// Gets or sets whether rows containing invalid cells should be included in the returned records.
		/// If set to <c>false</c>, only fully valid rows are returned.
		/// Default is <c>true</c>.
		/// </summary>
		public bool InsertInvalidRows { get; set; } = true;

		/// <summary>
		/// Gets a list of all records (rows) that contained at least one invalid cell during the last <see cref="ReadRecords"/> call.
		/// Each <see cref="Record"/> contains the row number and a list of the failed <see cref="Cell"/>s.
		/// </summary>
		public List<Record> FailedRecords { get; } = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvParser"/> class for the specified CSV file.
		/// </summary>
		/// <param name="filePath">The path to the CSV file to be parsed.</param>
		/// <param name="delimiter">The field delimiter. Defaults to ";".</param>
		/// <param name="culture">The culture used for parsing. Defaults to <see cref="CultureInfo.InvariantCulture"/>.</param>
		public CsvParser(string filePath, string delimiter = ";", CultureInfo? culture = null)
		{
			_filePath = filePath;
			_configuration = new CsvConfiguration(culture ?? CultureInfo.InvariantCulture)
			{
				Delimiter = delimiter,
				ShouldQuote = args => false,
				BadDataFound = args =>
				{
					_handlingBadDataFound = true;
				},

				// Here we remove empty headers and ignore them for stability
				// Missing fields must not throw an exception
				// example 1:
				// "Header1;Header2;;Header4" will ignore the empty header
				// example: 2:
				// "Header1;Header2;Header3;" will ignore the empty header at the end as the delimiter should not exist at the end of the line
				// MissingFieldFound is necessary to avoid exceptions when a field is missing in the CSV file
				// example 1.1:
				// Header1;Header2;;Header4
				// Val1,Val2,,Val3
				// example 2.1:
				// Header1;Header2;Header3;
				// Val1,Val2,Val3,
				PrepareHeaderForMatch = args =>
				{
					if (string.IsNullOrWhiteSpace(args.Header))
					{
						throw new CsvParserException($"Empty header found on index {args.FieldIndex}");
					}
					return args.Header;
				},
			};
		}

		/// <summary>
		/// Loads the CSV file and reads the header row.
		/// This method must be called before reading records.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the parser is already loaded.</exception>
		/// <exception cref="CsvParserException">Thrown if the header cannot be read.</exception>
		public void Load()
		{
			if (_loaded)
				throw new InvalidOperationException("CSV parser has already been loaded. Call Load() only once.");

			try
			{
				_reader = new StreamReader(_filePath);
				_csvReader = new CsvReader(_reader, _configuration);

				_csvReader.Read();
				_csvReader.ReadHeader();

				_loaded = true;
			}
			catch (CsvHelperException ex)
			{
				throw new CsvParserException("Failed to read CSV header.", ex);
			}
		}

		/// <summary>
		/// Returns the column headers from the CSV file.
		/// </summary>
		/// <returns>An array of column names.</returns>
		/// <exception cref="CsvParserException">Thrown if the header is missing or empty.</exception>
		public string[] GetHeaders()
		{
			EnsureLoaded();

			string[]? headers = _csvReader?.HeaderRecord;
			if (headers is null || headers.Length == 0)
			{
				throw new CsvParserException("CSV header is empty or not found.");
			}
			return headers;
		}

		/// <summary>
		/// Reads up to <paramref name="rowCount"/> records from the CSV file.
		/// Each record contains the row number and a list of cells (column name and value).
		/// Invalid cells are detected and handled according to <see cref="UseInvalidPlaceholder"/>.
		/// Rows with invalid cells are included or excluded based on <see cref="InsertInvalidRows"/>.
		/// All failed records are collected in <see cref="FailedRecords"/>.
		/// </summary>
		/// <param name="rowCount">The maximum number of rows to read. Default is 200.</param>
		/// <returns>A list of <see cref="Record"/> objects representing the parsed rows.</returns>
		public List<Record> ReadRecords(int rowCount = 200)
		{
			EnsureLoaded();

			var reader = _csvReader!;
			FailedRecords.Clear();
			var records = new List<Record>();
			int count = 0;
			string[] headers = GetHeaders();

			while (count < rowCount && reader.Read())
			{
				var cells = new List<Cell>(headers.Length);
				var failedCells = new List<Cell>();
				bool rowHasError = false;

				foreach (var header in headers)
				{
					string? value = reader.GetField(header);

					if (_handlingBadDataFound)
					{
						failedCells.Add(new Cell
						{
							Column = header,
							Value = value
						});

						if (UseInvalidPlaceholder)
							value = "[INVALID]";

						rowHasError = true;
					}

					cells.Add(new Cell
					{
						Column = header,
						Value = value
					});

					_handlingBadDataFound = false; // Reset after handling
				}

				if (rowHasError)
				{
					FailedRecords.Add(new Record
					{
						Row = reader.Context.Parser!.Row,
						Cells = failedCells
					});
				}

				if (!rowHasError || InsertInvalidRows)
				{
					records.Add(new Record
					{
						Row = reader.Context.Parser!.Row,
						Cells = cells
					});
				}
				count++;
			}
			return records;
		}

		/// <summary>
		/// Ensures that the CSV file has been loaded before performing operations.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the parser is not loaded.</exception>
		private void EnsureLoaded()
		{
			if (!_loaded)
				throw new InvalidOperationException("CSV parser has not been loaded. Call Load() before reading data.");
		}

		/// <summary>
		/// Releases all resources used by the <see cref="CsvParser"/>.
		/// </summary>
		public void Dispose()
		{
			_csvReader?.Dispose();
			_reader?.Dispose();
		}
	}


	/// <summary>
	/// Represents a single row in the CSV file, including its row number and all cells.
	/// </summary>
	public class Record
	{
		/// <summary>
		/// Gets or sets the row number in the CSV file (1-based).
		/// </summary>
		public long Row { get; set; }

		/// <summary>
		/// Gets or sets the list of cells in this row.
		/// </summary>
		public List<Cell> Cells { get; set; } = new();

		/// <summary>
		/// Converts this record to a dictionary mapping column names to cell values.
		/// </summary>
		/// <returns>A dictionary with column names as keys and cell values as values.</returns>
		public Dictionary<string, object?> ToDictionary()
		{
			return Cells.ToDictionary(cell => cell.Column, cell => (object?)cell.Value);
		}
	}

	/// <summary>
	/// Represents a single cell in a CSV row, including the column name and value.
	/// </summary>
	public class Cell
	{
		/// <summary>
		/// Gets or sets the column name for this cell.
		/// </summary>
		public string Column { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the value of this cell.
		/// </summary>
		public string? Value { get; set; }
	}
}

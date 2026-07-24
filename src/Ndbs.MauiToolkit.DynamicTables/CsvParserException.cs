namespace Ndbs.MauiToolkit.DynamicTables
{
	public class CsvParserException : Exception
	{
		public CsvParserException(string message, Exception? innerException = null)
			: base(message, innerException)
		{
		}
	}
}

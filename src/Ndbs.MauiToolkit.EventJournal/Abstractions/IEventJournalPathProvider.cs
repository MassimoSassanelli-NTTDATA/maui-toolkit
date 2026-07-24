namespace Ndbs.MauiToolkit.EventJournal.Abstractions
{
    /// <summary>
    /// Supplies the file system path of the SQLite event database. The journal
    /// library is storage-agnostic: the host decides where the database lives (for
    /// example inside the active tenant workspace). Returning <see langword="null"/>
    /// or an empty string means "no location available yet"; the SQLite sink then
    /// skips persistence for that event instead of failing.
    /// </summary>
    public interface IEventJournalPathProvider
    {
        /// <summary>Returns the database file path, or <see langword="null"/> when unavailable.</summary>
        string? GetDatabasePath();
    }
}

using Ndbs.MauiToolkit.EventJournal.Abstractions;

namespace Ndbs.MauiToolkit.EventJournal.Journaling
{
    /// <summary>
    /// An <see cref="IEventJournalPathProvider"/> that resolves the database path from
    /// a delegate. This lets a host wire the location (for example from a tenant
    /// workspace) without the journal library depending on that host concern.
    /// </summary>
    public sealed class DelegateEventJournalPathProvider : IEventJournalPathProvider
    {
        private readonly Func<string?> _resolve;

        /// <summary>Initializes a new instance of the <see cref="DelegateEventJournalPathProvider"/> class.</summary>
        /// <param name="resolve">The delegate that returns the database path (or <see langword="null"/>).</param>
        public DelegateEventJournalPathProvider(Func<string?> resolve)
            => _resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));

        /// <inheritdoc />
        public string? GetDatabasePath() => _resolve();
    }
}

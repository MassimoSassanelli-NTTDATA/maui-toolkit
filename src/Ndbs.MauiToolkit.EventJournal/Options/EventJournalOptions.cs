namespace Ndbs.MauiToolkit.EventJournal.Options
{
    /// <summary>
    /// Governance options for the event journal. They keep the journal a curated,
    /// bounded store rather than an unbounded data lake: size limits, retention and
    /// an optional category allow-list.
    /// </summary>
    public sealed class EventJournalOptions
    {
        /// <summary>Enables or disables journaling globally. Defaults to <see langword="true"/>.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The maximum UTF-8 size of <c>PayloadJson</c>. Larger payloads are truncated
        /// by the sanitizer. A value of <c>0</c> disables the limit. Defaults to 8 KB.
        /// </summary>
        public int MaxPayloadBytes { get; set; } = 8 * 1024;

        /// <summary>
        /// The maximum age of persisted events in days. Older events are purged by the
        /// SQLite sink. A value of <c>0</c> disables age-based retention. Defaults to 30.
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// The maximum number of persisted events. The SQLite sink purges the oldest
        /// events beyond this count. A value of <c>0</c> disables count-based retention.
        /// Defaults to 20,000.
        /// </summary>
        public int MaxRows { get; set; } = 20_000;

        /// <summary>
        /// Controls whether exception message and type are persisted. When
        /// <see langword="false"/>, only the stable <c>ErrorCode</c> is kept (message
        /// and type are dropped). Defaults to <see langword="true"/>.
        /// </summary>
        public bool CaptureExceptions { get; set; } = true;

        /// <summary>
        /// An optional allow-list of categories. When non-empty, events whose category
        /// is not contained here are dropped. When empty (the default), all categories
        /// are allowed.
        /// </summary>
        public ISet<string> AllowedCategories { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}

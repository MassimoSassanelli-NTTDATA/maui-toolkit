namespace Ndbs.MauiToolkit.EventJournal.Model
{
    /// <summary>
    /// The immutable, self-describing record of a single journaled event. It is the
    /// one shared shape every scenario uses: stable core fields for filtering and a
    /// free-form <see cref="PayloadJson"/> for scenario-specific detail (kept within
    /// the configured size limit).
    /// </summary>
    public sealed record EventEnvelope
    {
        /// <summary>The unique identifier of this event.</summary>
        public Guid EventId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Groups related events (for example all events of one synchronization run).
        /// </summary>
        public Guid CorrelationId { get; init; }

        /// <summary>The high-level category (for example "Sync", "Tenant", "Env").</summary>
        public required string Category { get; init; }

        /// <summary>The event name within the category (for example "RunStarted").</summary>
        public required string Name { get; init; }

        /// <summary>The outcome of the event.</summary>
        public EventOutcome Outcome { get; init; } = EventOutcome.Succeeded;

        /// <summary>When the event (or the operation it describes) started, in UTC.</summary>
        public DateTimeOffset StartedUtc { get; init; }

        /// <summary>When the operation ended, in UTC; <see langword="null"/> for point-in-time events.</summary>
        public DateTimeOffset? EndedUtc { get; init; }

        /// <summary>The measured duration in milliseconds, when applicable.</summary>
        public long? DurationMs { get; init; }

        /// <summary>The ambient context (tenant, user hash, app version, platform).</summary>
        public EventContext Context { get; init; } = EventContext.Empty;

        /// <summary>Optional scenario-specific detail as a compact JSON string.</summary>
        public string? PayloadJson { get; init; }

        /// <summary>An optional, stable error code for classification (for example "ApiDownloadFailed").</summary>
        public string? ErrorCode { get; init; }

        /// <summary>An optional human-readable error message (only stored when capture is enabled).</summary>
        public string? ErrorMessage { get; init; }

        /// <summary>The exception type name, when the event carries an exception.</summary>
        public string? ExceptionType { get; init; }
    }
}

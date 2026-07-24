using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Persistence
{
    /// <summary>
    /// The relational shape of a journaled event (one row per event). The ambient
    /// context is flattened into columns so the store stays queryable, while the
    /// scenario-specific detail remains in <see cref="PayloadJson"/>.
    /// </summary>
    public sealed class EventRecord
    {
        /// <summary>The auto-incrementing primary key (also the insertion order).</summary>
        public long Id { get; set; }

        /// <summary>The unique event identifier.</summary>
        public Guid EventId { get; set; }

        /// <summary>The correlation id grouping related events.</summary>
        public Guid CorrelationId { get; set; }

        /// <summary>The event category.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>The event name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>The outcome, stored as its integer value.</summary>
        public int Outcome { get; set; }

        /// <summary>When the operation started, in UTC.</summary>
        public DateTimeOffset StartedUtc { get; set; }

        /// <summary>When the operation ended, in UTC (nullable).</summary>
        public DateTimeOffset? EndedUtc { get; set; }

        /// <summary>The measured duration in milliseconds (nullable).</summary>
        public long? DurationMs { get; set; }

        /// <summary>The tenant id, when present.</summary>
        public Guid? TenantId { get; set; }

        /// <summary>The hashed user identity, when present.</summary>
        public string? UserIdHash { get; set; }

        /// <summary>The app version, when present.</summary>
        public string? AppVersion { get; set; }

        /// <summary>The platform, when present.</summary>
        public string? Platform { get; set; }

        /// <summary>The sanitized payload JSON, when present.</summary>
        public string? PayloadJson { get; set; }

        /// <summary>The stable error code, when present.</summary>
        public string? ErrorCode { get; set; }

        /// <summary>The error message, when present and capture is enabled.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>The exception type name, when present and capture is enabled.</summary>
        public string? ExceptionType { get; set; }

        /// <summary>Creates a record from an event envelope.</summary>
        public static EventRecord FromEnvelope(EventEnvelope envelope)
        {
            ArgumentNullException.ThrowIfNull(envelope);

            return new EventRecord
            {
                EventId = envelope.EventId,
                CorrelationId = envelope.CorrelationId,
                Category = envelope.Category,
                Name = envelope.Name,
                Outcome = (int)envelope.Outcome,
                StartedUtc = envelope.StartedUtc,
                EndedUtc = envelope.EndedUtc,
                DurationMs = envelope.DurationMs,
                TenantId = envelope.Context.TenantId,
                UserIdHash = envelope.Context.UserIdHash,
                AppVersion = envelope.Context.AppVersion,
                Platform = envelope.Context.Platform,
                PayloadJson = envelope.PayloadJson,
                ErrorCode = envelope.ErrorCode,
                ErrorMessage = envelope.ErrorMessage,
                ExceptionType = envelope.ExceptionType,
            };
        }

        /// <summary>Rebuilds the event envelope from this record.</summary>
        public EventEnvelope ToEnvelope() => new()
        {
            EventId = EventId,
            CorrelationId = CorrelationId,
            Category = Category,
            Name = Name,
            Outcome = (EventOutcome)Outcome,
            StartedUtc = StartedUtc,
            EndedUtc = EndedUtc,
            DurationMs = DurationMs,
            Context = new EventContext
            {
                TenantId = TenantId,
                UserIdHash = UserIdHash,
                AppVersion = AppVersion,
                Platform = Platform,
            },
            PayloadJson = PayloadJson,
            ErrorCode = ErrorCode,
            ErrorMessage = ErrorMessage,
            ExceptionType = ExceptionType,
        };
    }
}

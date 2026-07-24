using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Journaling
{
    /// <summary>
    /// Convenience helpers for recording point-in-time events without building an
    /// <see cref="EventEnvelope"/> by hand.
    /// </summary>
    public static class EventJournalExtensions
    {
        /// <summary>Records a single point-in-time event.</summary>
        /// <param name="journal">The journal.</param>
        /// <param name="category">The event category.</param>
        /// <param name="name">The event name.</param>
        /// <param name="outcome">The outcome; defaults to <see cref="EventOutcome.Succeeded"/>.</param>
        /// <param name="context">The context, or <see langword="null"/> for none.</param>
        /// <param name="payloadJson">Optional scenario-specific payload.</param>
        /// <param name="errorCode">An optional stable error code.</param>
        /// <param name="exception">An optional exception to record.</param>
        /// <param name="correlationId">A correlation id, or <see langword="null"/> to generate one.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public static Task RecordAsync(
            this IEventJournal journal,
            string category,
            string name,
            EventOutcome outcome = EventOutcome.Succeeded,
            EventContext? context = null,
            string? payloadJson = null,
            string? errorCode = null,
            Exception? exception = null,
            Guid? correlationId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(journal);
            ArgumentException.ThrowIfNullOrWhiteSpace(category);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var envelope = new EventEnvelope
            {
                CorrelationId = correlationId ?? Guid.NewGuid(),
                Category = category,
                Name = name,
                Outcome = outcome,
                Context = context ?? EventContext.Empty,
                PayloadJson = payloadJson,
                ErrorCode = errorCode,
                ErrorMessage = exception?.Message,
                ExceptionType = exception?.GetType().FullName,
            };

            return journal.WriteAsync(envelope, cancellationToken);
        }
    }
}

using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Abstractions
{
    /// <summary>
    /// The entry point for recording events. It applies the governance options,
    /// enrichment and payload sanitization, then fans the event out to all registered
    /// <see cref="IEventSink"/> instances. Writing is best-effort: the journal never
    /// throws back to the caller because of a sink failure.
    /// </summary>
    public interface IEventJournal
    {
        /// <summary>Records a single, already-built event.</summary>
        /// <param name="envelope">The event to record.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task WriteAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a timed scope for an operation. Completing the returned scope writes
        /// a single event with the measured duration and the chosen outcome.
        /// </summary>
        /// <param name="category">The event category.</param>
        /// <param name="name">The event name.</param>
        /// <param name="context">The initial context, or <see langword="null"/> for none.</param>
        /// <param name="correlationId">A correlation id, or <see langword="null"/> to generate one.</param>
        IEventScope BeginScope(
            string category,
            string name,
            EventContext? context = null,
            Guid? correlationId = null);
    }
}

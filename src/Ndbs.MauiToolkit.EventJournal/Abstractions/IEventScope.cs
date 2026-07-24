using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Abstractions
{
    /// <summary>
    /// A running measurement of an operation. Created via
    /// <see cref="IEventJournal.BeginScope"/>, it captures the start time and, on
    /// completion, writes a single event carrying the measured duration and outcome.
    /// A scope writes at most once; further completions are ignored.
    /// </summary>
    public interface IEventScope
    {
        /// <summary>The correlation id shared by this scope and any related events.</summary>
        Guid CorrelationId { get; }

        /// <summary>Replaces the context that will be written when the scope completes.</summary>
        void SetContext(EventContext context);

        /// <summary>Sets the payload that will be written when the scope completes.</summary>
        void SetPayload(string? payloadJson);

        /// <summary>Completes the scope and writes the event with the given outcome.</summary>
        Task CompleteAsync(
            EventOutcome outcome = EventOutcome.Succeeded,
            string? payloadJson = null,
            string? errorCode = null,
            CancellationToken cancellationToken = default);

        /// <summary>Completes the scope as failed and records the exception.</summary>
        Task FailAsync(
            Exception exception,
            string? errorCode = null,
            string? payloadJson = null,
            CancellationToken cancellationToken = default);
    }
}

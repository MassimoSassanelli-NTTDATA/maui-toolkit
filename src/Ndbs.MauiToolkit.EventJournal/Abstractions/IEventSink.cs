using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Abstractions
{
    /// <summary>
    /// A destination that persists or forwards a journaled event. Multiple sinks can
    /// be registered; the journal writes each event to all of them. A sink must not
    /// throw for expected conditions; failures are isolated by the journal so one
    /// failing sink never affects the others or the calling code.
    /// </summary>
    public interface IEventSink
    {
        /// <summary>Writes a single event to the sink.</summary>
        /// <param name="envelope">The prepared event to write.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task WriteAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);
    }
}

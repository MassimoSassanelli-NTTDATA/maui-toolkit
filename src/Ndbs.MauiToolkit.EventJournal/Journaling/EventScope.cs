using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Journaling
{
    /// <summary>
    /// The default <see cref="IEventScope"/>. It records the start timestamp on
    /// creation and, on completion, writes a single event carrying the measured
    /// duration and the chosen outcome. Duration is measured with the injected
    /// <see cref="TimeProvider"/> so it is monotonic and deterministic under test.
    /// </summary>
    public sealed class EventScope : IEventScope
    {
        private readonly IEventJournal _journal;
        private readonly TimeProvider _timeProvider;
        private readonly string _category;
        private readonly string _name;
        private readonly long _startTimestamp;
        private readonly DateTimeOffset _startedUtc;
        private EventContext _context;
        private string? _payloadJson;
        private bool _completed;

        /// <summary>Initializes a new instance of the <see cref="EventScope"/> class.</summary>
        public EventScope(
            IEventJournal journal,
            TimeProvider timeProvider,
            string category,
            string name,
            EventContext context,
            Guid correlationId)
        {
            _journal = journal ?? throw new ArgumentNullException(nameof(journal));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _category = category ?? throw new ArgumentNullException(nameof(category));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _context = context ?? EventContext.Empty;
            CorrelationId = correlationId;

            _startTimestamp = timeProvider.GetTimestamp();
            _startedUtc = timeProvider.GetUtcNow();
        }

        /// <inheritdoc />
        public Guid CorrelationId { get; }

        /// <inheritdoc />
        public void SetContext(EventContext context) => _context = context ?? _context;

        /// <inheritdoc />
        public void SetPayload(string? payloadJson) => _payloadJson = payloadJson;

        /// <inheritdoc />
        public Task CompleteAsync(
            EventOutcome outcome = EventOutcome.Succeeded,
            string? payloadJson = null,
            string? errorCode = null,
            CancellationToken cancellationToken = default)
            => WriteAsync(outcome, payloadJson, errorCode, exception: null, cancellationToken);

        /// <inheritdoc />
        public Task FailAsync(
            Exception exception,
            string? errorCode = null,
            string? payloadJson = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return WriteAsync(EventOutcome.Failed, payloadJson, errorCode, exception, cancellationToken);
        }

        private Task WriteAsync(
            EventOutcome outcome,
            string? payloadJson,
            string? errorCode,
            Exception? exception,
            CancellationToken cancellationToken)
        {
            if (_completed)
            {
                return Task.CompletedTask;
            }

            _completed = true;

            var elapsed = _timeProvider.GetElapsedTime(_startTimestamp);
            var envelope = new EventEnvelope
            {
                CorrelationId = CorrelationId,
                Category = _category,
                Name = _name,
                Outcome = outcome,
                StartedUtc = _startedUtc,
                EndedUtc = _timeProvider.GetUtcNow(),
                DurationMs = (long)elapsed.TotalMilliseconds,
                Context = _context,
                PayloadJson = payloadJson ?? _payloadJson,
                ErrorCode = errorCode,
                ErrorMessage = exception?.Message,
                ExceptionType = exception?.GetType().FullName,
            };

            return _journal.WriteAsync(envelope, cancellationToken);
        }
    }
}

using Microsoft.Extensions.Logging;
using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Model;
using Ndbs.MauiToolkit.EventJournal.Options;

namespace Ndbs.MauiToolkit.EventJournal.Journaling
{
    /// <summary>
    /// The default <see cref="IEventJournal"/>. It gates events by the configured
    /// options, applies enrichment and payload sanitization, then writes each event to
    /// every registered <see cref="IEventSink"/>. Sink failures are isolated and never
    /// propagate to the caller.
    /// </summary>
    public sealed class EventJournal : IEventJournal
    {
        private readonly IReadOnlyList<IEventSink> _sinks;
        private readonly IReadOnlyList<IEventEnricher> _enrichers;
        private readonly IEventPayloadSanitizer _sanitizer;
        private readonly EventJournalOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<EventJournal> _logger;

        /// <summary>Initializes a new instance of the <see cref="EventJournal"/> class.</summary>
        public EventJournal(
            IEnumerable<IEventSink> sinks,
            IEnumerable<IEventEnricher> enrichers,
            IEventPayloadSanitizer sanitizer,
            EventJournalOptions options,
            TimeProvider timeProvider,
            ILogger<EventJournal> logger)
        {
            ArgumentNullException.ThrowIfNull(sinks);
            ArgumentNullException.ThrowIfNull(enrichers);
            _sinks = sinks.ToArray();
            _enrichers = enrichers.ToArray();
            _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task WriteAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
        {
            if (envelope is null || !_options.Enabled)
            {
                return;
            }

            if (_options.AllowedCategories.Count > 0 && !_options.AllowedCategories.Contains(envelope.Category))
            {
                return;
            }

            var prepared = Prepare(envelope);

            foreach (var sink in _sinks)
            {
                try
                {
                    await sink.WriteAsync(prepared, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Event sink {Sink} failed for event {Category}/{Name}.",
                        sink.GetType().Name,
                        prepared.Category,
                        prepared.Name);
                }
            }
        }

        /// <inheritdoc />
        public IEventScope BeginScope(
            string category,
            string name,
            EventContext? context = null,
            Guid? correlationId = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(category);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new EventScope(
                this,
                _timeProvider,
                category,
                name,
                context ?? EventContext.Empty,
                correlationId ?? Guid.NewGuid());
        }

        private EventEnvelope Prepare(EventEnvelope envelope)
        {
            var context = envelope.Context ?? EventContext.Empty;
            foreach (var enricher in _enrichers)
            {
                context = enricher.Enrich(context) ?? context;
            }

            var payload = _sanitizer.Sanitize(envelope.PayloadJson, _options.MaxPayloadBytes);
            var startedUtc = envelope.StartedUtc == default ? _timeProvider.GetUtcNow() : envelope.StartedUtc;

            return envelope with
            {
                Context = context,
                PayloadJson = payload,
                StartedUtc = startedUtc,
                ErrorMessage = _options.CaptureExceptions ? envelope.ErrorMessage : null,
                ExceptionType = _options.CaptureExceptions ? envelope.ExceptionType : null,
            };
        }
    }
}

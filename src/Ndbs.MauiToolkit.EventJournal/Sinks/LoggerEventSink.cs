using Microsoft.Extensions.Logging;
using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Sinks
{
    /// <summary>
    /// An <see cref="IEventSink"/> that forwards events to <see cref="ILogger"/>. It
    /// maps the outcome to a log level and emits structured properties so events are
    /// queryable in any logging backend. It is always safe to enable (no I/O of its
    /// own beyond the configured logging providers).
    /// </summary>
    public sealed class LoggerEventSink : IEventSink
    {
        private readonly ILogger<LoggerEventSink> _logger;

        /// <summary>Initializes a new instance of the <see cref="LoggerEventSink"/> class.</summary>
        /// <param name="logger">The logger.</param>
        public LoggerEventSink(ILogger<LoggerEventSink> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public Task WriteAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
        {
            var level = envelope.Outcome switch
            {
                EventOutcome.Failed => LogLevel.Error,
                EventOutcome.Warning => LogLevel.Warning,
                EventOutcome.Cancelled => LogLevel.Warning,
                _ => LogLevel.Information,
            };

            _logger.Log(
                level,
                "Event {Category}/{Name} [{Outcome}] corr={CorrelationId} tenant={TenantId} durationMs={DurationMs} errorCode={ErrorCode}",
                envelope.Category,
                envelope.Name,
                envelope.Outcome,
                envelope.CorrelationId,
                envelope.Context.TenantId,
                envelope.DurationMs,
                envelope.ErrorCode);

            return Task.CompletedTask;
        }
    }
}

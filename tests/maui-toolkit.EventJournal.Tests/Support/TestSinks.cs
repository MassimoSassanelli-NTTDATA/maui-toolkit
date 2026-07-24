using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Tests.Support
{
    /// <summary>A simple sink that records every event it receives.</summary>
    internal sealed class RecordingEventSink : IEventSink
    {
        public List<EventEnvelope> Events { get; } = new();

        public Task WriteAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
        {
            Events.Add(envelope);
            return Task.CompletedTask;
        }
    }

    /// <summary>A sink that always throws, to verify failure isolation.</summary>
    internal sealed class ThrowingEventSink : IEventSink
    {
        public Task WriteAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("sink boom");
    }

    /// <summary>An enricher that stamps a fixed app version onto the context.</summary>
    internal sealed class AppVersionEnricher : IEventEnricher
    {
        private readonly string _version;

        public AppVersionEnricher(string version) => _version = version;

        public EventContext Enrich(EventContext context)
            => context with { AppVersion = context.AppVersion ?? _version };
    }
}

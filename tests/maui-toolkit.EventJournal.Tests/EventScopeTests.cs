using Microsoft.Extensions.Logging.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Model;
using Ndbs.MauiToolkit.EventJournal.Options;
using Ndbs.MauiToolkit.EventJournal.Sanitization;
using Ndbs.MauiToolkit.EventJournal.Tests.Support;
using Xunit;

namespace Ndbs.MauiToolkit.EventJournal.Tests
{
    public class EventScopeTests
    {
        private static readonly DateTimeOffset Now = new(2026, 7, 24, 8, 0, 0, TimeSpan.Zero);

        private static (Journaling.EventJournal Journal, RecordingEventSink Sink, FakeTimeProvider Time) Create()
        {
            var sink = new RecordingEventSink();
            var time = new FakeTimeProvider(Now);
            var journal = new Journaling.EventJournal(
                new IEventSink[] { sink },
                Array.Empty<IEventEnricher>(),
                new DefaultEventPayloadSanitizer(),
                new EventJournalOptions(),
                time,
                NullLogger<Journaling.EventJournal>.Instance);
            return (journal, sink, time);
        }

        [Fact]
        public async Task CompleteAsync_MeasuresDuration_AndWritesOutcome()
        {
            var (journal, sink, time) = Create();

            var scope = journal.BeginScope("Sync", "Run");
            time.Advance(TimeSpan.FromMilliseconds(500));
            await scope.CompleteAsync(EventOutcome.Succeeded);

            var written = Assert.Single(sink.Events);
            Assert.Equal(EventOutcome.Succeeded, written.Outcome);
            Assert.Equal(500, written.DurationMs);
            Assert.Equal(Now, written.StartedUtc);
            Assert.Equal(Now.AddMilliseconds(500), written.EndedUtc);
            Assert.Equal(scope.CorrelationId, written.CorrelationId);
        }

        [Fact]
        public async Task FailAsync_WritesFailedOutcome_AndException()
        {
            var (journal, sink, _) = Create();

            var scope = journal.BeginScope("Sync", "Run");
            await scope.FailAsync(new InvalidOperationException("boom"), errorCode: "SyncFailed");

            var written = Assert.Single(sink.Events);
            Assert.Equal(EventOutcome.Failed, written.Outcome);
            Assert.Equal("SyncFailed", written.ErrorCode);
            Assert.Equal("boom", written.ErrorMessage);
            Assert.Equal(typeof(InvalidOperationException).FullName, written.ExceptionType);
        }

        [Fact]
        public async Task Complete_CalledTwice_WritesOnlyOnce()
        {
            var (journal, sink, _) = Create();

            var scope = journal.BeginScope("Sync", "Run");
            await scope.CompleteAsync();
            await scope.CompleteAsync(EventOutcome.Failed);

            Assert.Single(sink.Events);
            Assert.Equal(EventOutcome.Succeeded, sink.Events[0].Outcome);
        }

        [Fact]
        public async Task BeginScope_UsesProvidedCorrelationId()
        {
            var (journal, sink, _) = Create();
            var correlationId = Guid.NewGuid();

            var scope = journal.BeginScope("Sync", "Run", correlationId: correlationId);
            await scope.CompleteAsync();

            Assert.Equal(correlationId, scope.CorrelationId);
            Assert.Equal(correlationId, sink.Events[0].CorrelationId);
        }

        [Fact]
        public async Task SetPayload_IsWrittenOnCompletion()
        {
            var (journal, sink, _) = Create();

            var scope = journal.BeginScope("Sync", "Run");
            scope.SetPayload("{\"added\":2}");
            await scope.CompleteAsync();

            Assert.Equal("{\"added\":2}", sink.Events[0].PayloadJson);
        }
    }
}

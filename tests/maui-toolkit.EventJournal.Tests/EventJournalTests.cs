using Microsoft.Extensions.Logging.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Journaling;
using Ndbs.MauiToolkit.EventJournal.Model;
using Ndbs.MauiToolkit.EventJournal.Options;
using Ndbs.MauiToolkit.EventJournal.Sanitization;
using Ndbs.MauiToolkit.EventJournal.Tests.Support;
using Xunit;

namespace Ndbs.MauiToolkit.EventJournal.Tests
{
    public class EventJournalTests
    {
        private static readonly DateTimeOffset Now = new(2026, 7, 24, 8, 0, 0, TimeSpan.Zero);

        private static Journaling.EventJournal CreateJournal(
            EventJournalOptions options,
            out RecordingEventSink sink,
            IEnumerable<IEventEnricher>? enrichers = null,
            IEventPayloadSanitizer? sanitizer = null)
        {
            sink = new RecordingEventSink();
            return new Journaling.EventJournal(
                new IEventSink[] { sink },
                enrichers ?? Array.Empty<IEventEnricher>(),
                sanitizer ?? new DefaultEventPayloadSanitizer(),
                options,
                new FakeTimeProvider(Now),
                NullLogger<Journaling.EventJournal>.Instance);
        }

        private static EventEnvelope Envelope(string category = "Sync", string name = "RunStarted") => new()
        {
            Category = category,
            Name = name,
        };

        [Fact]
        public async Task WriteAsync_WhenDisabled_DoesNotWriteToSinks()
        {
            var journal = CreateJournal(new EventJournalOptions { Enabled = false }, out var sink);

            await journal.WriteAsync(Envelope());

            Assert.Empty(sink.Events);
        }

        [Fact]
        public async Task WriteAsync_WithAllowList_DropsForeignCategories()
        {
            var options = new EventJournalOptions();
            options.AllowedCategories.Add("Sync");
            var journal = CreateJournal(options, out var sink);

            await journal.WriteAsync(Envelope(category: "Other"));
            await journal.WriteAsync(Envelope(category: "Sync"));

            Assert.Single(sink.Events);
            Assert.Equal("Sync", sink.Events[0].Category);
        }

        [Fact]
        public async Task WriteAsync_AppliesEnrichers_ToContext()
        {
            var journal = CreateJournal(
                new EventJournalOptions(),
                out var sink,
                enrichers: new IEventEnricher[] { new AppVersionEnricher("1.4.0") });

            await journal.WriteAsync(Envelope());

            Assert.Equal("1.4.0", sink.Events[0].Context.AppVersion);
        }

        [Fact]
        public async Task WriteAsync_DefaultsStartedUtc_FromTimeProvider()
        {
            var journal = CreateJournal(new EventJournalOptions(), out var sink);

            await journal.WriteAsync(Envelope());

            Assert.Equal(Now, sink.Events[0].StartedUtc);
        }

        [Fact]
        public async Task WriteAsync_WhenCaptureExceptionsDisabled_DropsMessageAndTypeButKeepsCode()
        {
            var journal = CreateJournal(new EventJournalOptions { CaptureExceptions = false }, out var sink);

            await journal.WriteAsync(Envelope() with
            {
                ErrorCode = "ApiDownloadFailed",
                ErrorMessage = "secret detail",
                ExceptionType = "System.Net.Http.HttpRequestException",
            });

            var written = sink.Events[0];
            Assert.Equal("ApiDownloadFailed", written.ErrorCode);
            Assert.Null(written.ErrorMessage);
            Assert.Null(written.ExceptionType);
        }

        [Fact]
        public async Task WriteAsync_FansOutToAllSinks()
        {
            var first = new RecordingEventSink();
            var second = new RecordingEventSink();
            var journal = new Journaling.EventJournal(
                new IEventSink[] { first, second },
                Array.Empty<IEventEnricher>(),
                new DefaultEventPayloadSanitizer(),
                new EventJournalOptions(),
                new FakeTimeProvider(Now),
                NullLogger<Journaling.EventJournal>.Instance);

            await journal.WriteAsync(Envelope());

            Assert.Single(first.Events);
            Assert.Single(second.Events);
        }

        [Fact]
        public async Task WriteAsync_WhenOneSinkThrows_StillWritesToOthersAndDoesNotThrow()
        {
            var healthy = new RecordingEventSink();
            var journal = new Journaling.EventJournal(
                new IEventSink[] { new ThrowingEventSink(), healthy },
                Array.Empty<IEventEnricher>(),
                new DefaultEventPayloadSanitizer(),
                new EventJournalOptions(),
                new FakeTimeProvider(Now),
                NullLogger<Journaling.EventJournal>.Instance);

            await journal.WriteAsync(Envelope());

            Assert.Single(healthy.Events);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Journaling;
using Ndbs.MauiToolkit.EventJournal.Model;
using Ndbs.MauiToolkit.EventJournal.Options;
using Ndbs.MauiToolkit.EventJournal.Persistence;
using Ndbs.MauiToolkit.EventJournal.Tests.Support;
using Xunit;

namespace Ndbs.MauiToolkit.EventJournal.Tests
{
    public sealed class SqliteEventSinkTests : IDisposable
    {
        private static readonly DateTimeOffset Now = new(2026, 7, 24, 8, 0, 0, TimeSpan.Zero);

        private readonly string _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"eventjournal-tests-{Guid.NewGuid():N}.db");

        private SqliteEventSink CreateSink(EventJournalOptions options, TimeProvider? time = null)
            => new(
                new DelegateEventJournalPathProvider(() => _dbPath),
                options,
                time ?? new FakeTimeProvider(Now),
                NullLogger<SqliteEventSink>.Instance);

        private EventJournalDbContext CreateReadContext()
        {
            var contextOptions = new DbContextOptionsBuilder<EventJournalDbContext>()
                .UseSqlite($"Filename={_dbPath}")
                .Options;
            return new EventJournalDbContext(contextOptions);
        }

        private static EventEnvelope Envelope(string name, DateTimeOffset? startedUtc = null) => new()
        {
            CorrelationId = Guid.NewGuid(),
            Category = "Sync",
            Name = name,
            Outcome = EventOutcome.Succeeded,
            StartedUtc = startedUtc ?? Now,
            Context = new EventContext { TenantId = Guid.NewGuid(), AppVersion = "1.0" },
            PayloadJson = "{\"n\":1}",
        };

        [Fact]
        public async Task WriteAsync_PersistsEvent_AndRoundTrips()
        {
            var sink = CreateSink(new EventJournalOptions { RetentionDays = 0, MaxRows = 0 });
            var envelope = Envelope("RunCompleted");

            await sink.WriteAsync(envelope);

            using var context = CreateReadContext();
            var record = Assert.Single(await context.Events.AsNoTracking().ToListAsync());
            Assert.Equal(envelope.EventId, record.EventId);
            Assert.Equal("Sync", record.Category);
            Assert.Equal("RunCompleted", record.Name);
            Assert.Equal(envelope.Context.TenantId, record.TenantId);
            Assert.Equal("{\"n\":1}", record.PayloadJson);
        }

        [Fact]
        public async Task WriteAsync_WhenNoPath_SkipsPersistence()
        {
            var sink = new SqliteEventSink(
                new DelegateEventJournalPathProvider(() => null),
                new EventJournalOptions(),
                new FakeTimeProvider(Now),
                NullLogger<SqliteEventSink>.Instance);

            await sink.WriteAsync(Envelope("RunCompleted"));

            Assert.False(File.Exists(_dbPath));
        }

        [Fact]
        public async Task WriteAsync_EnforcesMaxRows_KeepingMostRecent()
        {
            var sink = CreateSink(new EventJournalOptions { RetentionDays = 0, MaxRows = 2 });

            await sink.WriteAsync(Envelope("first"));
            await sink.WriteAsync(Envelope("second"));
            await sink.WriteAsync(Envelope("third"));

            using var context = CreateReadContext();
            var names = await context.Events.AsNoTracking()
                .OrderBy(e => e.Id)
                .Select(e => e.Name)
                .ToListAsync();

            Assert.Equal(new[] { "second", "third" }, names);
        }

        [Fact]
        public async Task WriteAsync_EnforcesRetentionDays_PurgingOldEvents()
        {
            var sink = CreateSink(new EventJournalOptions { RetentionDays = 1, MaxRows = 0 });

            await sink.WriteAsync(Envelope("recent", startedUtc: Now));
            await sink.WriteAsync(Envelope("old", startedUtc: Now.AddDays(-2)));

            using var context = CreateReadContext();
            var record = Assert.Single(await context.Events.AsNoTracking().ToListAsync());
            Assert.Equal("recent", record.Name);
        }

        public void Dispose()
        {
            foreach (var file in new[] { _dbPath, _dbPath + "-wal", _dbPath + "-shm" })
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (IOException)
                {
                    // Best-effort cleanup of temporary test artifacts.
                }
            }
        }
    }
}

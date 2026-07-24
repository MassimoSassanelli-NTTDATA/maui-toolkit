using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Model;
using Ndbs.MauiToolkit.EventJournal.Options;

namespace Ndbs.MauiToolkit.EventJournal.Persistence
{
    /// <summary>
    /// An <see cref="IEventSink"/> that persists events into a dedicated SQLite
    /// database, whose location is supplied by the host through
    /// <see cref="IEventJournalPathProvider"/>. It creates the database and schema on
    /// first use per path, and enforces the configured retention (by age and by row
    /// count) after every write. When no path is available, persistence is skipped.
    /// </summary>
    public sealed class SqliteEventSink : IEventSink
    {
        private readonly IEventJournalPathProvider _pathProvider;
        private readonly EventJournalOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<SqliteEventSink> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly HashSet<string> _initializedPaths = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Initializes a new instance of the <see cref="SqliteEventSink"/> class.</summary>
        public SqliteEventSink(
            IEventJournalPathProvider pathProvider,
            EventJournalOptions options,
            TimeProvider timeProvider,
            ILogger<SqliteEventSink> logger)
        {
            _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task WriteAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(envelope);

            var path = _pathProvider.GetDatabasePath();
            if (string.IsNullOrWhiteSpace(path))
            {
                // No location available yet (for example before a tenant is active).
                return;
            }

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await using var context = CreateContext(path);
                await EnsureInitializedAsync(context, path, cancellationToken).ConfigureAwait(false);

                context.Events.Add(EventRecord.FromEnvelope(envelope));
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                await EnforceRetentionAsync(context, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }

        private static EventJournalDbContext CreateContext(string path)
        {
            var options = new DbContextOptionsBuilder<EventJournalDbContext>()
                .UseSqlite($"Filename={path}")
                .Options;

            return new EventJournalDbContext(options);
        }

        private async Task EnsureInitializedAsync(EventJournalDbContext context, string path, CancellationToken cancellationToken)
        {
            if (_initializedPaths.Contains(path))
            {
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
            _initializedPaths.Add(path);
        }

        private async Task EnforceRetentionAsync(EventJournalDbContext context, CancellationToken cancellationToken)
        {
            if (_options.RetentionDays > 0)
            {
                var cutoff = _timeProvider.GetUtcNow() - TimeSpan.FromDays(_options.RetentionDays);
                var expiredIds = await context.Events
                    .Where(e => e.StartedUtc < cutoff)
                    .Select(e => e.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (expiredIds.Count > 0)
                {
                    await context.Events
                        .Where(e => expiredIds.Contains(e.Id))
                        .ExecuteDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            if (_options.MaxRows > 0)
            {
                var count = await context.Events.CountAsync(cancellationToken).ConfigureAwait(false);
                if (count > _options.MaxRows)
                {
                    var overflow = count - _options.MaxRows;
                    var ids = await context.Events
                        .OrderBy(e => e.Id)
                        .Take(overflow)
                        .Select(e => e.Id)
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await context.Events
                        .Where(e => ids.Contains(e.Id))
                        .ExecuteDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ndbs.MauiToolkit.EventJournal.Persistence
{
    /// <summary>
    /// The dedicated EF Core context for the diagnostics event database (its own
    /// SQLite file, separate from any business database). It holds a single table of
    /// <see cref="EventRecord"/> rows with indexes that support the common queries
    /// (by time, correlation and category).
    /// </summary>
    public sealed class EventJournalDbContext : DbContext
    {
        // SQLite has no native DateTimeOffset type and cannot translate range
        // comparisons on it. Storing the UTC ticks as an integer keeps retention
        // filtering and ordering translatable and efficient.
        private static readonly ValueConverter<DateTimeOffset, long> UtcTicksConverter = new(
            value => value.UtcTicks,
            value => new DateTimeOffset(value, TimeSpan.Zero));

        private static readonly ValueConverter<DateTimeOffset?, long?> NullableUtcTicksConverter = new(
            value => value.HasValue ? value.Value.UtcTicks : null,
            value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);

        /// <summary>Initializes a new instance of the <see cref="EventJournalDbContext"/> class.</summary>
        /// <param name="options">The context options.</param>
        public EventJournalDbContext(DbContextOptions<EventJournalDbContext> options)
            : base(options)
        {
        }

        /// <summary>Gets the set of journaled events.</summary>
        public DbSet<EventRecord> Events => Set<EventRecord>();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventRecord>(entity =>
            {
                entity.ToTable("EventJournal");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.StartedUtc).HasConversion(UtcTicksConverter);
                entity.Property(e => e.EndedUtc).HasConversion(NullableUtcTicksConverter);

                entity.HasIndex(e => e.StartedUtc);
                entity.HasIndex(e => e.CorrelationId);
                entity.HasIndex(e => e.Category);
            });
        }
    }
}

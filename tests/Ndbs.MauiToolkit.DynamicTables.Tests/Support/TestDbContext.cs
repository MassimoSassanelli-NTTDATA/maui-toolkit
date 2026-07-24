using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ndbs.MauiToolkit.DynamicTables.Tests.Support
{
    /// <summary>
    /// A test entity used to exercise <see cref="DynamicTableRepository.Query{T}"/>.
    /// Its property names match the columns of the dynamically created test table.
    /// </summary>
    public class TestPerson
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    /// <summary>
    /// Minimal EF Core <see cref="DbContext"/> backed by SQLite for the dynamic table tests.
    /// </summary>
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestPerson> People => Set<TestPerson>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestPerson>(builder =>
            {
                builder.HasKey(p => p.Id);
                builder.ToTable("People");
            });
        }
    }

    /// <summary>
    /// Provides a <see cref="TestDbContext"/> backed by a private, open in-memory SQLite
    /// connection. The connection is kept open for the lifetime of the fixture so that the
    /// in-memory database survives across the (open/close) calls performed by the repository.
    /// </summary>
    public sealed class SqliteTestFixture : IDisposable
    {
        private readonly SqliteConnection _connection;

        public SqliteTestFixture()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        public TestDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new TestDbContext(options);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}

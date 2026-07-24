using Microsoft.EntityFrameworkCore;
using Ndbs.MauiToolkit.Xia.MicroApps.Model;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Persistence
{
    /// <summary>
    /// The dedicated EF Core context for the per-tenant <c>microapp.db</c> database
    /// (§7.3), kept separate from the app's maintenance database. It stores the micro
    /// app master data, custom properties, forms, form settings and the three kinds of
    /// file references. The dynamic content tables (§5.4) are created in the same
    /// database through <c>IDynamicTableImportService</c>.
    /// </summary>
    public sealed class MicroAppDbContext : DbContext
    {
        /// <summary>Initializes a new instance of the <see cref="MicroAppDbContext"/> class.</summary>
        /// <param name="options">The context options.</param>
        public MicroAppDbContext(DbContextOptions<MicroAppDbContext> options)
            : base(options)
        {
        }

        /// <summary>Gets the set of micro apps.</summary>
        public DbSet<MicroApp> MicroApps => Set<MicroApp>();

        /// <summary>Gets the set of micro app custom properties.</summary>
        public DbSet<MicroAppEigenschaft> MicroAppEigenschaften => Set<MicroAppEigenschaft>();

        /// <summary>Gets the set of forms.</summary>
        public DbSet<Formular> Formulare => Set<Formular>();

        /// <summary>Gets the set of form settings.</summary>
        public DbSet<FormularEinstellung> FormularEinstellungen => Set<FormularEinstellung>();

        /// <summary>Gets the set of completion file references.</summary>
        public DbSet<VervollstaendigungsDatei> Vervollstaendigungen => Set<VervollstaendigungsDatei>();

        /// <summary>Gets the set of text file references.</summary>
        public DbSet<TextDatei> Texte => Set<TextDatei>();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MicroApp>(entity =>
            {
                entity.HasKey(e => e.Name);
                entity.Property(e => e.Name).IsRequired();

                entity.HasMany(e => e.Eigenschaften)
                    .WithOne()
                    .HasForeignKey(e => e.MicroAppName)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Formulare)
                    .WithOne()
                    .HasForeignKey(e => e.MicroAppName)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MicroAppEigenschaft>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Schluessel).IsRequired();
            });

            modelBuilder.Entity<Formular>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired();
                entity.HasIndex(e => new { e.MicroAppName, e.Name }).IsUnique();

                entity.HasMany(e => e.Einstellungen)
                    .WithOne()
                    .HasForeignKey(e => e.FormularId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Vervollstaendigungen)
                    .WithOne()
                    .HasForeignKey(e => e.FormularId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Texte)
                    .WithOne()
                    .HasForeignKey(e => e.FormularId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FormularEinstellung>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Schluessel).IsRequired();
            });

            modelBuilder.Entity<VervollstaendigungsDatei>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Dateiname).IsRequired();
            });

            modelBuilder.Entity<TextDatei>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Dateiname).IsRequired();
            });
        }
    }
}

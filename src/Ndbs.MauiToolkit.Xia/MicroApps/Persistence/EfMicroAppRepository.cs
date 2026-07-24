using Microsoft.EntityFrameworkCore;
using Ndbs.MauiToolkit.Xia.MicroApps.Model;
using Ndbs.MauiToolkit.Xia.MicroApps.Sync;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Persistence
{
    /// <summary>
    /// EF Core backed <see cref="IMicroAppRepository"/> over the tenant
    /// <see cref="MicroAppDbContext"/> (<c>microapp.db</c>).
    /// </summary>
    public sealed class EfMicroAppRepository : IMicroAppRepository
    {
        private readonly MicroAppDbContext _context;

        /// <summary>Initializes a new instance of the <see cref="EfMicroAppRepository"/> class.</summary>
        /// <param name="context">The micro app database context.</param>
        public EfMicroAppRepository(MicroAppDbContext context)
            => _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <inheritdoc />
        public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
            => _context.Database.EnsureCreatedAsync(cancellationToken);

        /// <inheritdoc />
        public async Task<IReadOnlyList<MicroAppLocalState>> GetLocalStatesAsync(CancellationToken cancellationToken = default)
        {
            var states = await _context.MicroApps
                .AsNoTracking()
                .Select(app => new MicroAppLocalState(app.Name, app.ETag, app.IstSynchronisiert))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return states;
        }

        /// <inheritdoc />
        public async Task<MicroApp?> FindAsync(string name, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return await _context.MicroApps
                .Include(app => app.Eigenschaften)
                .Include(app => app.Formulare).ThenInclude(form => form.Einstellungen)
                .Include(app => app.Formulare).ThenInclude(form => form.Vervollstaendigungen)
                .Include(app => app.Formulare).ThenInclude(form => form.Texte)
                .FirstOrDefaultAsync(app => app.Name == name, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task AddAsync(MicroApp microApp, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(microApp);

            _context.MicroApps.Add(microApp);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string name, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var existing = await _context.MicroApps
                .FirstOrDefaultAsync(app => app.Name == name, cancellationToken)
                .ConfigureAwait(false);

            if (existing is null)
            {
                return;
            }

            // Dependent rows (custom properties, forms and their file references) are
            // removed by the configured cascade delete (§5.5).
            _context.MicroApps.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

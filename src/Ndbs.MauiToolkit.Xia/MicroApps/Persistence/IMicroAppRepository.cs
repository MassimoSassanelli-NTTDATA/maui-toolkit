using Ndbs.MauiToolkit.Xia.MicroApps.Model;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Persistence
{
    /// <summary>
    /// Persistence gateway for the micro app master data held in <c>microapp.db</c>
    /// (§5.1–5.3, §7.3). Implementations wrap the dedicated EF Core context; the
    /// interface stays free of EF Core so the synchronization orchestration can be unit
    /// tested with a fake repository.
    /// </summary>
    public interface IMicroAppRepository
    {
        /// <summary>Ensures the <c>microapp.db</c> schema exists. Idempotent.</summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task EnsureCreatedAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets the lightweight local state of every stored micro app (§3.2).</summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The local micro app states.</returns>
        Task<IReadOnlyList<Sync.MicroAppLocalState>> GetLocalStatesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads a micro app together with its forms and file references, or
        /// <see langword="null"/> when it does not exist.
        /// </summary>
        /// <param name="name">The micro app name (key).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The micro app graph, or <see langword="null"/>.</returns>
        Task<MicroApp?> FindAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>Adds a micro app graph and persists it.</summary>
        /// <param name="microApp">The micro app graph to add.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task AddAsync(MicroApp microApp, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a micro app and its dependent rows (cascade: micro app → form →
        /// file references). Does nothing when the micro app does not exist.
        /// </summary>
        /// <param name="name">The micro app name (key).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task RemoveAsync(string name, CancellationToken cancellationToken = default);
    }
}

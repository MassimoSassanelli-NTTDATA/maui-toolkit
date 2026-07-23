namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Stores the list of system environments and the per-user active selection.
    /// Environment names are the identity; comparisons are case-insensitive.
    /// </summary>
    public interface ISystemEnvironmentStore
    {
        /// <summary>Gets all persisted environments.</summary>
        Task<IReadOnlyList<SystemEnvironment>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Finds an environment by its unique name, or <see langword="null"/>.</summary>
        /// <param name="name">The environment name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<SystemEnvironment?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new environment or replaces an existing one with the same name.
        /// </summary>
        /// <param name="environment">The environment to persist.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task AddOrUpdateAsync(SystemEnvironment environment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the environment with the given name. When it was the active
        /// selection, the active selection is cleared as well.
        /// </summary>
        /// <param name="name">The environment name.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task RemoveAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the active environment for the given scope, or <see langword="null"/>.
        /// A stored selection that no longer exists is treated as absent and cleared.
        /// </summary>
        /// <param name="userId">The user identity, or <see langword="null"/> before login (device scope).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<SystemEnvironment?> GetActiveAsync(string? userId, CancellationToken cancellationToken = default);

        /// <summary>Persists the active environment selection for the given scope.</summary>
        /// <param name="userId">The user identity, or <see langword="null"/> before login (device scope).</param>
        /// <param name="name">The environment name to activate.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task SetActiveAsync(string? userId, string name, CancellationToken cancellationToken = default);

        /// <summary>Clears the active environment selection for the given scope.</summary>
        /// <param name="userId">The user identity, or <see langword="null"/> before login (device scope).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task ClearActiveAsync(string? userId, CancellationToken cancellationToken = default);
    }
}

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Orchestrates a controlled switch of the active system environment. A switch
    /// re-targets the whole working context (identity provider, API addresses,
    /// tenant, environment-scoped caches), not just a value, and must always go
    /// through this coordinator rather than by applying configuration directly.
    /// </summary>
    public interface ISystemEnvironmentSwitchCoordinator
    {
        /// <summary>
        /// Switches the active environment to <paramref name="target"/> in a
        /// deterministic order: validate → publish changing → reset all components
        /// (reverse order) → apply present sections (order) → persist active selection
        /// → publish changed.
        /// </summary>
        /// <param name="target">The environment to switch to.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<EnvironmentSwitchResult> SwitchAsync(SystemEnvironment target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Switches the active environment to <paramref name="target"/> using explicit
        /// <paramref name="options"/> that describe the caller's intent (for example
        /// whether an active user session should be reset). The order is identical to
        /// <see cref="SwitchAsync(SystemEnvironment, CancellationToken)"/>; the options
        /// are forwarded to each component's reset.
        /// </summary>
        /// <param name="target">The environment to switch to.</param>
        /// <param name="options">The switch options describing the caller's intent.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<EnvironmentSwitchResult> SwitchAsync(SystemEnvironment target, EnvironmentSwitchOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies the configuration of an already-active environment during startup
        /// without any tear-down (no logout, no cache clearing) and without changing
        /// the persisted selection. Used to re-establish the runtime configuration
        /// (identity provider, API addresses) after an app restart.
        /// </summary>
        /// <param name="environment">The active environment whose configuration to apply.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task ApplyActiveAsync(SystemEnvironment environment, CancellationToken cancellationToken = default);
    }
}

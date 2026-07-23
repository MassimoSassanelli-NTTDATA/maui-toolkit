namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Default <see cref="ISystemEnvironmentStartupResolver"/> implementing the start
    /// flow required by the environment feature:
    /// <list type="bullet">
    /// <item>no environment stored → <see cref="EnvironmentStartupDecision.Capture"/>,</item>
    /// <item>environments stored but none active → <see cref="EnvironmentStartupDecision.ShowSelection"/>,</item>
    /// <item>a valid active environment → <see cref="EnvironmentStartupDecision.Continue"/>.</item>
    /// </list>
    /// A stored active selection that no longer exists is treated as absent by the
    /// store, so it can never leak an invalid selection into the start flow.
    /// </summary>
    public sealed class SystemEnvironmentStartupResolver : ISystemEnvironmentStartupResolver
    {
        private readonly ISystemEnvironmentStore _store;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEnvironmentStartupResolver"/> class.
        /// </summary>
        /// <param name="store">The environment store.</param>
        public SystemEnvironmentStartupResolver(ISystemEnvironmentStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <inheritdoc />
        public async Task<EnvironmentStartupResult> ResolveAsync(string? userId, CancellationToken cancellationToken = default)
        {
            var all = await _store.GetAllAsync(cancellationToken).ConfigureAwait(false);
            if (all.Count == 0)
            {
                return EnvironmentStartupResult.Capture();
            }

            var active = await _store.GetActiveAsync(userId, cancellationToken).ConfigureAwait(false);
            return active is null
                ? EnvironmentStartupResult.ShowSelection()
                : EnvironmentStartupResult.Continue(active);
        }
    }
}

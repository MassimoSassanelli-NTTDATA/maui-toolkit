namespace Ndbs.MauiToolkit.Xia.Data
{
    /// <summary>
    /// A no-op <see cref="ITenantScopedCache"/> used until tenant-scoped caching is
    /// introduced.
    /// </summary>
    public sealed class NoOpTenantScopedCache : ITenantScopedCache
    {
        /// <inheritdoc />
        public Task ClearAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

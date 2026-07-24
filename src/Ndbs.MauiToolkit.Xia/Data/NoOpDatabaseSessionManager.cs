namespace Ndbs.MauiToolkit.Xia.Data
{
    /// <summary>
    /// A no-op <see cref="IDatabaseSessionManager"/> used until the tenant-level
    /// database wiring is implemented.
    /// </summary>
    public sealed class NoOpDatabaseSessionManager : IDatabaseSessionManager
    {
        /// <inheritdoc />
        public Task CloseCurrentAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

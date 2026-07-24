namespace Ndbs.MauiToolkit.Xia.Navigation
{
    /// <summary>
    /// A no-op <see cref="ITenantNavigator"/> default. Apps should register their own
    /// implementation that resets the navigation stack to their tenant home route.
    /// </summary>
    public sealed class NoOpTenantNavigator : ITenantNavigator
    {
        /// <inheritdoc />
        public Task NavigateToTenantHomeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

namespace Ndbs.MauiToolkit.Xia.Navigation
{
    /// <summary>
    /// Resets navigation to the tenant home after a tenant switch. Because routes are
    /// app-specific, the app provides the concrete implementation; the toolkit ships a
    /// safe no-op default.
    /// </summary>
    public interface ITenantNavigator
    {
        /// <summary>
        /// Resets the navigation stack and navigates to the tenant home / dashboard.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task NavigateToTenantHomeAsync(CancellationToken cancellationToken = default);
    }
}

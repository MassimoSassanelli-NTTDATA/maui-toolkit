namespace Ndbs.MauiToolkit.Xia.MicroApps.Sync
{
    /// <summary>
    /// Progress information reported during a micro app synchronization run (§7.1). The
    /// loading step maps <see cref="Ratio"/> to its progress bar and shows
    /// <see cref="StatusText"/> as the current phase.
    /// </summary>
    /// <param name="Completed">The number of micro apps processed so far.</param>
    /// <param name="Total">The total number of micro apps to process.</param>
    /// <param name="CurrentMicroApp">The micro app currently being processed, if any.</param>
    /// <param name="StatusText">A human-readable status describing the current phase.</param>
    public sealed record MicroAppSyncProgress(
        int Completed,
        int Total,
        string? CurrentMicroApp,
        string StatusText)
    {
        /// <summary>Gets the completion ratio in the range 0..1.</summary>
        public double Ratio => Total <= 0 ? 1d : Math.Clamp((double)Completed / Total, 0d, 1d);
    }
}

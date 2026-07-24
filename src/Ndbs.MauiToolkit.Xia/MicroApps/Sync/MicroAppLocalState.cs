namespace Ndbs.MauiToolkit.Xia.MicroApps.Sync
{
    /// <summary>
    /// The lightweight local state of a micro app used to compare the local store with
    /// the server state (specification §3.2).
    /// </summary>
    /// <param name="Name">The micro app name (key).</param>
    /// <param name="ETag">The locally stored server version tag.</param>
    /// <param name="IstSynchronisiert">
    /// Whether the local content is fully in sync. <see langword="false"/> marks the
    /// micro app as faulty and forces a re-synchronization.
    /// </param>
    public sealed record MicroAppLocalState(
        string Name,
        string? ETag,
        bool IstSynchronisiert);
}

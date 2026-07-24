namespace Ndbs.MauiToolkit.Xia.MicroApps.Sync
{
    /// <summary>
    /// The outcome of comparing the server state with the local state (§3.2). It holds
    /// the five categories and the derived sets of micro apps to add, update or remove.
    /// Faulty micro apps are folded into <see cref="ToUpdate"/>.
    /// </summary>
    public sealed class MicroAppSyncDiff
    {
        /// <summary>Gets the micro apps present on the server but not locally.</summary>
        public required IReadOnlyList<MicroAppServerImage> Added { get; init; }

        /// <summary>Gets the micro apps present locally but no longer on the server.</summary>
        public required IReadOnlyList<string> Removed { get; init; }

        /// <summary>Gets the micro apps present on both sides with a different ETag.</summary>
        public required IReadOnlyList<MicroAppServerImage> Changed { get; init; }

        /// <summary>Gets the micro apps present on both sides with an identical ETag and a healthy local state.</summary>
        public required IReadOnlyList<MicroAppServerImage> Unchanged { get; init; }

        /// <summary>Gets the micro apps present locally but not fully synchronized (faulty).</summary>
        public required IReadOnlyList<MicroAppServerImage> Faulted { get; init; }

        /// <summary>Gets the micro apps to add.</summary>
        public IReadOnlyList<MicroAppServerImage> ToAdd => Added;

        /// <summary>
        /// Gets the micro apps to update: the changed ones plus the faulty ones that
        /// have to be repaired (§3.2).
        /// </summary>
        public IReadOnlyList<MicroAppServerImage> ToUpdate { get; init; } = Array.Empty<MicroAppServerImage>();

        /// <summary>Gets the micro app names to remove.</summary>
        public IReadOnlyList<string> ToRemove => Removed;

        /// <summary>
        /// Gets a value indicating whether there is any difference to apply. When
        /// <see langword="false"/> the sync ends without touching the database (§3.2).
        /// </summary>
        public bool HasChanges => Added.Count > 0 || Removed.Count > 0 || ToUpdate.Count > 0;
    }
}

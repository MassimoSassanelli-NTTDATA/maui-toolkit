namespace Ndbs.MauiToolkit.Xia.MicroApps.Sync
{
    /// <summary>
    /// Compares the server state with the local state and produces a
    /// <see cref="MicroAppSyncDiff"/> with the five categories described in the
    /// specification (§3.2). Micro app names are matched case-sensitively (they are the
    /// tenant-wide unique identifiers).
    /// </summary>
    public static class MicroAppDiffCalculator
    {
        /// <summary>Computes the difference between the server and local states.</summary>
        /// <param name="serverImages">The merged server-side micro app images.</param>
        /// <param name="localStates">The local micro app states.</param>
        /// <returns>The computed difference.</returns>
        public static MicroAppSyncDiff Compute(
            IEnumerable<MicroAppServerImage> serverImages,
            IEnumerable<MicroAppLocalState> localStates)
        {
            ArgumentNullException.ThrowIfNull(serverImages);
            ArgumentNullException.ThrowIfNull(localStates);

            var server = serverImages.ToList();
            var local = localStates.ToDictionary(state => state.Name, StringComparer.Ordinal);
            var serverNames = new HashSet<string>(server.Select(image => image.Name), StringComparer.Ordinal);

            var added = new List<MicroAppServerImage>();
            var changed = new List<MicroAppServerImage>();
            var unchanged = new List<MicroAppServerImage>();
            var faulted = new List<MicroAppServerImage>();

            foreach (var image in server)
            {
                if (!local.TryGetValue(image.Name, out var localState))
                {
                    added.Add(image);
                    continue;
                }

                if (!localState.IstSynchronisiert)
                {
                    // Locally present but incompletely synchronized → repair (§3.2).
                    faulted.Add(image);
                }
                else if (!string.Equals(localState.ETag, image.ETag, StringComparison.Ordinal))
                {
                    changed.Add(image);
                }
                else
                {
                    unchanged.Add(image);
                }
            }

            var removed = local.Keys
                .Where(name => !serverNames.Contains(name))
                .ToList();

            var toUpdate = new List<MicroAppServerImage>(changed.Count + faulted.Count);
            toUpdate.AddRange(changed);
            toUpdate.AddRange(faulted);

            return new MicroAppSyncDiff
            {
                Added = added,
                Removed = removed,
                Changed = changed,
                Unchanged = unchanged,
                Faulted = faulted,
                ToUpdate = toUpdate,
            };
        }
    }
}

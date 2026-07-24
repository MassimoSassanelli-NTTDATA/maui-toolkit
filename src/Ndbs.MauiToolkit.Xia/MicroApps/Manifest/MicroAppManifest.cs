namespace Ndbs.MauiToolkit.Xia.MicroApps.Manifest
{
    /// <summary>
    /// In-memory representation of a micro app manifest (<c>project.json</c>) as
    /// documented in the functional specification (§7.4). Only the fields relevant to
    /// this application are modelled; undeclared fields (for example
    /// <c>formuploadmethod</c> or <c>adapterarrangements</c>) are ignored during
    /// parsing.
    /// </summary>
    public sealed class MicroAppManifest
    {
        /// <summary>Gets the tenant-wide unique micro app name (key).</summary>
        public required string Name { get; init; }

        /// <summary>Gets the display name, or <see langword="null"/> when absent.</summary>
        public string? DisplayName { get; init; }

        /// <summary>Gets the description, or <see langword="null"/> when absent.</summary>
        public string? Description { get; init; }

        /// <summary>
        /// Gets the customer-specific properties. Values are scalars (null / number /
        /// string / boolean) serialized to their string representation.
        /// </summary>
        public IReadOnlyDictionary<string, string?> CustomProperties { get; init; }
            = new Dictionary<string, string?>();

        /// <summary>Gets the forms declared by the manifest.</summary>
        public IReadOnlyList<MicroAppManifestForm> Forms { get; init; }
            = Array.Empty<MicroAppManifestForm>();
    }
}

using Ndbs.MauiToolkit.Xia.MicroApps.Manifest;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Packaging
{
    /// <summary>
    /// Extracts downloaded micro app ZIP packages into the micro app workspace and
    /// reads the <c>project.json</c> manifest (§4, §7.4).
    /// </summary>
    public interface IMicroAppPackageService
    {
        /// <summary>
        /// Extracts the ZIP package into <paramref name="destinationRoot"/>. Any
        /// existing content in the destination is replaced so the micro app folder
        /// reflects exactly the downloaded package (§5.5).
        /// </summary>
        /// <param name="zipStream">The downloaded ZIP stream.</param>
        /// <param name="destinationRoot">The micro app workspace root path.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task ExtractAsync(Stream zipStream, string destinationRoot, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads and parses the <c>project.json</c> manifest from an extracted micro
        /// app folder.
        /// </summary>
        /// <param name="extractedRoot">The folder the package was extracted to.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The parsed manifest.</returns>
        Task<MicroAppManifest> ReadManifestAsync(string extractedRoot, CancellationToken cancellationToken = default);
    }
}

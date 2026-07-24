using System.IO.Compression;
using Ndbs.MauiToolkit.Xia.MicroApps.Manifest;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Packaging
{
    /// <summary>
    /// Default <see cref="IMicroAppPackageService"/>. It extracts the ZIP with a guard
    /// against path traversal ("zip slip") and reads <c>project.json</c> from the
    /// extracted folder.
    /// </summary>
    public sealed class MicroAppPackageService : IMicroAppPackageService
    {
        /// <summary>The manifest file name inside a micro app package.</summary>
        public const string ManifestFileName = "project.json";

        /// <inheritdoc />
        public async Task ExtractAsync(Stream zipStream, string destinationRoot, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(zipStream);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationRoot);

            if (Directory.Exists(destinationRoot))
            {
                Directory.Delete(destinationRoot, recursive: true);
            }

            Directory.CreateDirectory(destinationRoot);
            var normalizedRoot = Path.GetFullPath(destinationRoot);
            var rootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
                ? normalizedRoot
                : normalizedRoot + Path.DirectorySeparatorChar;

            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var targetPath = Path.GetFullPath(Path.Combine(normalizedRoot, entry.FullName));

                // Guard against path traversal (zip slip): the resolved path must stay
                // inside the destination directory.
                if (!targetPath.StartsWith(rootWithSeparator, StringComparison.Ordinal) &&
                    !string.Equals(targetPath, normalizedRoot, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"The package entry '{entry.FullName}' escapes the destination directory.");
                }

                // Directory entry.
                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(targetPath);
                    continue;
                }

                var directory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await using var entryStream = entry.Open();
                await using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await entryStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task<MicroAppManifest> ReadManifestAsync(string extractedRoot, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(extractedRoot);

            var manifestPath = Path.Combine(extractedRoot, ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                throw new MicroAppManifestException(
                    $"The package does not contain a '{ManifestFileName}' manifest.");
            }

            var json = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
            return MicroAppManifestParser.Parse(json);
        }
    }
}

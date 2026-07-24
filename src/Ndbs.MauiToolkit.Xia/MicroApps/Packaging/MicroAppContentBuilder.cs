using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ndbs.MauiToolkit.DynamicTables;
using Ndbs.MauiToolkit.Xia.MicroApps.Manifest;
using Ndbs.MauiToolkit.Xia.MicroApps.Model;
using Ndbs.MauiToolkit.Xia.MicroApps.Sync;

namespace Ndbs.MauiToolkit.Xia.MicroApps.Packaging
{
    /// <summary>
    /// Builds a <see cref="MicroApp"/> master-data graph from a parsed manifest and,
    /// for every <b>referenced</b> completion/text CSV that actually exists on disk,
    /// creates a dynamic content table through <see cref="IDynamicTableImportService"/>
    /// and records the generated table name on the file reference (§5.2–5.4).
    /// </summary>
    /// <remarks>
    /// Only files referenced by the manifest are imported; files that merely exist in
    /// the package are ignored. A referenced file that is missing leaves its file
    /// reference without a content table (§5.3).
    /// </remarks>
    public sealed class MicroAppContentBuilder
    {
        private readonly IDynamicTableImportService _importService;
        private readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="MicroAppContentBuilder"/> class.</summary>
        /// <param name="importService">The dynamic table import service (targeting <c>microapp.db</c>).</param>
        /// <param name="logger">An optional logger.</param>
        public MicroAppContentBuilder(IDynamicTableImportService importService, ILogger? logger = null)
        {
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>Builds the micro app graph and imports the referenced CSV content.</summary>
        /// <param name="image">The merged server image (name, display name, description, ETag).</param>
        /// <param name="manifest">The parsed manifest.</param>
        /// <param name="extractedRoot">The folder the package was extracted to.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The fully populated micro app graph.</returns>
        public async Task<MicroApp> BuildAsync(
            MicroAppServerImage image,
            MicroAppManifest manifest,
            string extractedRoot,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(image);
            ArgumentNullException.ThrowIfNull(manifest);
            ArgumentException.ThrowIfNullOrWhiteSpace(extractedRoot);

            var app = new MicroApp
            {
                Name = image.Name,
                Anzeigename = manifest.DisplayName ?? image.DisplayName,
                Beschreibung = manifest.Description ?? image.Description,
                ETag = image.ETag,
                LetzterDownload = DateTimeOffset.UtcNow,
                IstSynchronisiert = true,
            };

            foreach (var property in manifest.CustomProperties)
            {
                app.Eigenschaften.Add(new MicroAppEigenschaft
                {
                    MicroAppName = image.Name,
                    Schluessel = property.Key,
                    Wert = property.Value,
                });
            }

            foreach (var manifestForm in manifest.Forms)
            {
                var formFileName = Path.GetFileName(NormalizeRelativePath(manifestForm.Path));
                var form = new Formular
                {
                    MicroAppName = image.Name,
                    Name = manifestForm.Name,
                    Anzeigename = manifestForm.DisplayName,
                    Beschreibung = manifestForm.Description,
                    FormulardateiName = formFileName,
                    FormulardateiTyp = MimeTypeResolver.Resolve(formFileName),
                };

                foreach (var setting in manifestForm.Settings)
                {
                    form.Einstellungen.Add(new FormularEinstellung
                    {
                        Schluessel = setting.Key,
                        Wert = setting.Value,
                    });
                }

                foreach (var completion in manifestForm.Completions)
                {
                    var reference = await ImportReferenceAsync(
                        completion, extractedRoot, MicroAppTablePrefixes.Completion, cancellationToken)
                        .ConfigureAwait(false);

                    form.Vervollstaendigungen.Add(new VervollstaendigungsDatei
                    {
                        Dateipfad = reference.RelativePath,
                        Dateiname = reference.FileName,
                        Dateityp = reference.MimeType,
                        Inhaltstabelle = reference.TableName,
                    });
                }

                foreach (var text in manifestForm.Texts)
                {
                    var reference = await ImportReferenceAsync(
                        text, extractedRoot, MicroAppTablePrefixes.Text, cancellationToken)
                        .ConfigureAwait(false);

                    form.Texte.Add(new TextDatei
                    {
                        Dateipfad = reference.RelativePath,
                        Dateiname = reference.FileName,
                        Dateityp = reference.MimeType,
                        Inhaltstabelle = reference.TableName,
                    });
                }

                app.Formulare.Add(form);
            }

            return app;
        }

        private async Task<FileReference> ImportReferenceAsync(
            string relativePath,
            string extractedRoot,
            string tableNamePrefix,
            CancellationToken cancellationToken)
        {
            var normalized = NormalizeRelativePath(relativePath);
            var fileName = Path.GetFileName(normalized);
            var mimeType = MimeTypeResolver.Resolve(fileName);
            var absolutePath = Path.Combine(extractedRoot, ToPlatformPath(normalized));

            string? tableName = null;
            if (File.Exists(absolutePath))
            {
                var result = await _importService
                    .ImportCsvAsync(absolutePath, tableNamePrefix, cancellationToken)
                    .ConfigureAwait(false);
                tableName = result.TableName;
            }
            else
            {
                _logger.LogWarning(
                    "Referenced micro app content file '{RelativePath}' is missing; no content table created.",
                    normalized);
            }

            return new FileReference(normalized, fileName, mimeType, tableName);
        }

        private static string NormalizeRelativePath(string path)
        {
            var normalized = path.Replace('\\', '/').Trim();
            while (normalized.StartsWith("./", StringComparison.Ordinal))
            {
                normalized = normalized[2..];
            }

            return normalized.TrimStart('/');
        }

        private static string ToPlatformPath(string normalizedRelativePath)
            => normalizedRelativePath.Replace('/', Path.DirectorySeparatorChar);

        private readonly record struct FileReference(
            string RelativePath,
            string FileName,
            string MimeType,
            string? TableName);
    }
}

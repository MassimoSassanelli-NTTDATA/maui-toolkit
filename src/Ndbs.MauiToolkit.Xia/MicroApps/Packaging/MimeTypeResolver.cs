namespace Ndbs.MauiToolkit.Xia.MicroApps.Packaging
{
    /// <summary>
    /// Derives a MIME type from a file name (§5.3: the file type is derived from the
    /// file name). Only the file types relevant to micro app packages are mapped;
    /// unknown extensions fall back to <c>application/octet-stream</c>.
    /// </summary>
    public static class MimeTypeResolver
    {
        private const string Fallback = "application/octet-stream";

        private static readonly IReadOnlyDictionary<string, string> Map =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".xaml"] = "application/xaml+xml",
                [".xml"] = "application/xml",
                [".html"] = "text/html",
                [".htm"] = "text/html",
                [".css"] = "text/css",
                [".csv"] = "text/csv",
                [".json"] = "application/json",
                [".txt"] = "text/plain",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".gif"] = "image/gif",
                [".svg"] = "image/svg+xml",
            };

        /// <summary>Resolves the MIME type for a file name.</summary>
        /// <param name="fileName">The file name (or path).</param>
        /// <returns>The mapped MIME type, or the octet-stream fallback.</returns>
        public static string Resolve(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Fallback;
            }

            var extension = Path.GetExtension(fileName);
            return Map.TryGetValue(extension, out var mime) ? mime : Fallback;
        }
    }
}

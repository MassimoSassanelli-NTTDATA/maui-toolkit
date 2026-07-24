namespace Ndbs.MauiToolkit.Xia.MicroApps.Manifest
{
    /// <summary>
    /// Raised when a micro app manifest (<c>project.json</c>) cannot be parsed because
    /// it is invalid JSON or misses a required field.
    /// </summary>
    public sealed class MicroAppManifestException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="MicroAppManifestException"/> class.</summary>
        public MicroAppManifestException(string message) : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MicroAppManifestException"/> class.</summary>
        public MicroAppManifestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

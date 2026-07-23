using Microsoft.Maui.Storage;

namespace Ndbs.MauiToolkit.Workspace
{
    /// <summary>
    /// Default <see cref="IAppDataRootProvider"/> that uses the MAUI
    /// <see cref="FileSystem.AppDataDirectory"/>.
    /// </summary>
    public sealed class MauiAppDataRootProvider : IAppDataRootProvider
    {
        /// <inheritdoc />
        public string GetAppDataRoot() => FileSystem.AppDataDirectory;
    }
}

namespace Ndbs.MauiToolkit.Workspace
{
    /// <summary>
    /// Provides the root directory under which all per-user, per-tenant local data
    /// is stored. Abstracted so it can be replaced in tests (which must not depend
    /// on the MAUI <c>FileSystem.AppDataDirectory</c>).
    /// </summary>
    public interface IAppDataRootProvider
    {
        /// <summary>
        /// Gets the absolute path of the application data root directory.
        /// </summary>
        string GetAppDataRoot();
    }
}

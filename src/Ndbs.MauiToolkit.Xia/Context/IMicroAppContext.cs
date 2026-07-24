using Ndbs.MauiToolkit.Xia.Workspace;

namespace Ndbs.MauiToolkit.Xia.Context
{
    /// <summary>
    /// The central truth for the currently active micro app of the active tenant.
    /// View models and services obtain the active micro app from this context rather
    /// than passing its name and workspace around manually.
    /// </summary>
    /// <remarks>
    /// The micro app must never be changed by directly assigning a property. Use the
    /// context chain (activate the micro-app stage) to perform a controlled switch.
    /// </remarks>
    public interface IMicroAppContext
    {
        /// <summary>
        /// Gets the name of the currently active micro app, or <see langword="null"/>
        /// when none is active.
        /// </summary>
        string? CurrentMicroApp { get; }

        /// <summary>
        /// Gets the local workspace of the currently active micro app, or
        /// <see langword="null"/> when none is active.
        /// </summary>
        MicroAppWorkspace? CurrentWorkspace { get; }

        /// <summary>Gets a value indicating whether a micro app is currently active.</summary>
        bool HasMicroApp { get; }

        /// <summary>
        /// Sets the currently active micro app together with its workspace. Intended
        /// to be called by the micro-app stage only.
        /// </summary>
        /// <param name="microAppName">The micro app name.</param>
        /// <param name="workspace">The micro app workspace.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetCurrentMicroAppAsync(string microAppName, MicroAppWorkspace workspace, CancellationToken cancellationToken = default);

        /// <summary>Clears the currently active micro app.</summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}

using Ndbs.MauiToolkit.Xia.Workspace;

namespace Ndbs.MauiToolkit.Xia.Context
{
    /// <summary>
    /// Default in-memory implementation of <see cref="IMicroAppContext"/>.
    /// </summary>
    public sealed class MicroAppContext : IMicroAppContext
    {
        /// <inheritdoc />
        public string? CurrentMicroApp { get; private set; }

        /// <inheritdoc />
        public MicroAppWorkspace? CurrentWorkspace { get; private set; }

        /// <inheritdoc />
        public bool HasMicroApp => CurrentMicroApp is not null;

        /// <inheritdoc />
        public Task SetCurrentMicroAppAsync(string microAppName, MicroAppWorkspace workspace, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(microAppName);
            ArgumentNullException.ThrowIfNull(workspace);

            CurrentMicroApp = microAppName;
            CurrentWorkspace = workspace;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            CurrentMicroApp = null;
            CurrentWorkspace = null;
            return Task.CompletedTask;
        }
    }
}

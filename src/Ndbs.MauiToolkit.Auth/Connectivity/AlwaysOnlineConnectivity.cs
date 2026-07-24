namespace Ndbs.MauiToolkit.Auth.Connectivity
{
    /// <summary>
    /// Default <see cref="INetworkConnectivity"/> that always reports connectivity.
    /// Used as a fallback when the hosting app does not register its own network
    /// status implementation (A10).
    /// </summary>
    public sealed class AlwaysOnlineConnectivity : INetworkConnectivity
    {
        /// <inheritdoc />
        public bool IsConnected => true;
    }
}

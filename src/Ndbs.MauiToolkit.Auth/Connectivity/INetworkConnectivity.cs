namespace Ndbs.MauiToolkit.Auth.Connectivity
{
    /// <summary>
    /// Reports whether the device currently has network connectivity. Operations
    /// that require the internet (login, refresh) consult this before going online
    /// and fail fast with a meaningful result when offline (A10).
    /// </summary>
    /// <remarks>
    /// The library deliberately does not implement network detection itself; the
    /// hosting app provides an implementation (for example wrapping MAUI
    /// <c>Connectivity</c>). When no implementation is registered, the library uses
    /// <see cref="AlwaysOnlineConnectivity"/>.
    /// </remarks>
    public interface INetworkConnectivity
    {
        /// <summary>
        /// Gets a value indicating whether the device currently has network access.
        /// </summary>
        bool IsConnected { get; }
    }
}

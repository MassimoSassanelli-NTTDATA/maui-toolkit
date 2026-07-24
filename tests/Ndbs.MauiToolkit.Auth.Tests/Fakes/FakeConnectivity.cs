using Ndbs.MauiToolkit.Auth.Connectivity;

namespace Ndbs.MauiToolkit.Auth.Tests.Fakes
{
    /// <summary>Configurable <see cref="INetworkConnectivity"/> for tests.</summary>
    internal sealed class FakeConnectivity : INetworkConnectivity
    {
        public FakeConnectivity(bool isConnected = true) => IsConnected = isConnected;

        public bool IsConnected { get; set; }
    }
}

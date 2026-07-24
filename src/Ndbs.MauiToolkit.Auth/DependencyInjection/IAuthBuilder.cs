using Microsoft.Extensions.DependencyInjection;

namespace Ndbs.MauiToolkit.Auth.DependencyInjection
{
    /// <summary>
    /// Fluent builder returned by
    /// <see cref="AuthServiceCollectionExtensions.AddNdbsAuth"/> for further
    /// configuration of the authentication library.
    /// </summary>
    public interface IAuthBuilder
    {
        /// <summary>
        /// Gets the underlying service collection so additional services (such as a
        /// custom <see cref="Connectivity.INetworkConnectivity"/>) can be registered.
        /// </summary>
        IServiceCollection Services { get; }
    }
}

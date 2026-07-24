using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ndbs.MauiToolkit.Auth.Client;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Connectivity;
using Ndbs.MauiToolkit.Auth.Http;
using Ndbs.MauiToolkit.Auth.Maui;
using Ndbs.MauiToolkit.Auth.Providers;
using Ndbs.MauiToolkit.Auth.Tokens;
using ISecureStorage = Ndbs.MauiToolkit.Auth.Tokens.ISecureStorage;

namespace Ndbs.MauiToolkit.Auth.DependencyInjection
{
    /// <summary>
    /// Dependency-injection registration for the <c>Ndbs.MauiToolkit.Auth</c> OIDC
    /// authentication library. A single call activates and configures the library.
    /// </summary>
    public static class AuthServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the OIDC authentication services and validates the supplied
        /// configuration eagerly, so missing mandatory parameters fail fast (A17).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">A callback that populates the <see cref="OidcOptions"/>.</param>
        /// <returns>
        /// An <see cref="IAuthBuilder"/> that allows further wiring such as binding the
        /// bearer-token handler to an <see cref="HttpClient"/>.
        /// </returns>
        /// <exception cref="OidcConfigurationException">When the configuration is invalid (A17).</exception>
        public static IAuthBuilder AddNdbsAuth(this IServiceCollection services, Action<OidcOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            var options = new OidcOptions();
            configure(options);

            // Fail fast on invalid configuration (A17). The provider validates again.
            OidcOptionsValidator.Validate(options);
            services.AddSingleton<IOidcOptionsProvider>(_ => new OidcOptionsProvider(options));

            // Loosely coupled state notifications use the shared WeakReferenceMessenger (A15).
            services.TryAddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

            // The app provides network detection; fall back to "always online" (A10).
            services.TryAddSingleton<INetworkConnectivity, AlwaysOnlineConnectivity>();

            // Platform infrastructure.
            services.TryAddSingleton<ISecureStorage, MauiSecureStorage>();

            services.TryAddSingleton<ITokenStore, SecureStorageTokenStore>();

            // Provider-agnostic OIDC seam: the active identity provider is selected at
            // runtime (per environment) and the routing client delegates to it. Each
            // identity-provider add-on (for example AddIas()) registers one
            // IdentityProviderOidcClient.
            services.TryAddSingleton<IActiveIdentityProvider, ActiveIdentityProvider>();
            services.TryAddSingleton<IOidcClient>(sp => new RoutingOidcClient(
                sp.GetServices<IdentityProviderOidcClient>(),
                sp.GetRequiredService<IActiveIdentityProvider>()));

            // Singleton so the refresh lock is shared across the whole app (A4).
            services.TryAddSingleton<IAuthenticationService, AuthenticationService>();

            // Available for opt-in binding to an HttpClient (A14).
            services.TryAddTransient<BearerTokenHandler>();

            return new AuthBuilder(services);
        }

        private sealed class AuthBuilder : IAuthBuilder
        {
            public AuthBuilder(IServiceCollection services) => Services = services;

            public IServiceCollection Services { get; }
        }
    }
}

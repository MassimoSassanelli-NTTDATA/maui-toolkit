using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.DependencyInjection;
using Ndbs.MauiToolkit.Auth.Entra.Msal;
using Ndbs.MauiToolkit.Auth.Providers;
using Ndbs.MauiToolkit.Environments;

namespace Ndbs.MauiToolkit.Auth.Entra
{
    /// <summary>
    /// Dependency-injection registration for the Azure Entra ID identity provider
    /// (MSAL.NET). Call it on the <see cref="IAuthBuilder"/> returned by
    /// <c>AddNdbsAuth(...)</c> to make Entra available as an identity provider.
    /// </summary>
    public static class EntraAuthBuilderExtensions
    {
        /// <summary>
        /// Registers the Azure Entra ID identity provider: the MSAL public-client
        /// adapter, the Entra OIDC client, the routing registration that selects Entra,
        /// and the <see cref="EntraIdpComponent"/> that plugs Entra into the environment
        /// services.
        /// </summary>
        /// <param name="builder">The authentication builder from <c>AddNdbsAuth(...)</c>.</param>
        /// <returns>The same <see cref="IAuthBuilder"/> for chaining.</returns>
        public static IAuthBuilder AddEntra(this IAuthBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var services = builder.Services;

            // MSAL public-client seam (app can override with a parent-window-aware factory).
            services.TryAddSingleton<IMsalPublicClient>(sp =>
                new MsalPublicClientAdapter(sp.GetRequiredService<IOidcOptionsProvider>()));

            // Entra OIDC client backed by MSAL.
            services.TryAddSingleton<MsalEntraClient>();

            // Register the Entra client with the provider-routing seam.
            services.AddSingleton(sp => new IdentityProviderOidcClient(
                EntraIdpComponent.ProviderKey,
                sp.GetRequiredService<MsalEntraClient>()));

            // Plug the Entra identity provider into the environment services so it is
            // applied/reset during a controlled environment switch.
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystemEnvironmentComponent, EntraIdpComponent>());

            return builder;
        }
    }
}

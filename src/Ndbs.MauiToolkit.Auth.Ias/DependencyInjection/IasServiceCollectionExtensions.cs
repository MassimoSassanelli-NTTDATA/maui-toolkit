using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ndbs.MauiToolkit.Auth.DependencyInjection;
using Ndbs.MauiToolkit.Auth.Providers;
using Ndbs.MauiToolkit.Environments;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace Ndbs.MauiToolkit.Auth.Ias
{
    /// <summary>
    /// Dependency-injection registration for the IAS identity provider (SAP IAS via
    /// the Duende OIDC client and the MAUI system browser). Call it on the
    /// <see cref="IAuthBuilder"/> returned by
    /// <c>AddNdbsAuth(...)</c> to make IAS available as an identity provider.
    /// </summary>
    public static class IasAuthBuilderExtensions
    {
        /// <summary>
        /// Registers the IAS identity provider: the MAUI browsers for the system-browser
        /// Authorization Code Flow with PKCE, the Duende-based OIDC client, the routing
        /// registration that selects IAS, and the <see cref="IasIdpComponent"/> that
        /// plugs IAS into the environment services.
        /// </summary>
        /// <param name="builder">The authentication builder from <c>AddNdbsAuth(...)</c>.</param>
        /// <returns>The same <see cref="IAuthBuilder"/> for chaining.</returns>
        public static IAuthBuilder AddIas(this IAuthBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var services = builder.Services;

            // MAUI browsers for the interactive sign-in. The platform browser honours
            // OidcBrowserKind and forces the embedded web view on Windows (A11).
            services.TryAddSingleton<MauiAuthenticatorBrowser>();
            services.TryAddSingleton<MauiWebViewBrowser>();
            services.TryAddSingleton<IBrowser, MauiPlatformBrowser>();

            // Duende-based OIDC client for the IAS provider.
            services.TryAddSingleton<DuendeOidcClient>();

            // Register the IAS client with the provider-routing seam.
            services.AddSingleton(sp => new IdentityProviderOidcClient(
                IasIdpComponent.ProviderKey,
                sp.GetRequiredService<DuendeOidcClient>()));

            // Plug the IAS identity provider into the environment services so it is
            // applied/reset during a controlled environment switch.
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystemEnvironmentComponent, IasIdpComponent>());

            return builder;
        }
    }
}

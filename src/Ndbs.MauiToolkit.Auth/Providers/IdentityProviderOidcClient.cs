using Ndbs.MauiToolkit.Auth.Client;

namespace Ndbs.MauiToolkit.Auth.Providers
{
    /// <summary>
    /// Associates a concrete <see cref="IOidcClient"/> with the identity-provider key
    /// under which it is selected by the <see cref="RoutingOidcClient"/>. Each
    /// identity-provider add-on (for example the IAS or Azure Entra project)
    /// registers one instance of this type in dependency injection.
    /// </summary>
    public sealed class IdentityProviderOidcClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProviderOidcClient"/> class.
        /// </summary>
        /// <param name="providerKey">The identity-provider key (for example <c>IAS</c>).</param>
        /// <param name="client">The concrete OIDC client for that provider.</param>
        public IdentityProviderOidcClient(string providerKey, IOidcClient client)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
            ArgumentNullException.ThrowIfNull(client);

            ProviderKey = providerKey;
            Client = client;
        }

        /// <summary>Gets the identity-provider key that selects this client.</summary>
        public string ProviderKey { get; }

        /// <summary>Gets the concrete OIDC client for the provider.</summary>
        public IOidcClient Client { get; }
    }
}

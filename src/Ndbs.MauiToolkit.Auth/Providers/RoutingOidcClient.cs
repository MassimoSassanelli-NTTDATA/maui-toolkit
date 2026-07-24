using Ndbs.MauiToolkit.Auth.Client;

namespace Ndbs.MauiToolkit.Auth.Providers
{
    /// <summary>
    /// <see cref="IOidcClient"/> that routes every operation to the OIDC client of the
    /// currently active identity provider. This is the provider-agnostic seam that
    /// lets the base authentication library drive multiple identity providers (for
    /// example IAS and Azure Entra) while <see cref="IAuthenticationService"/> keeps
    /// depending on a single <see cref="IOidcClient"/>.
    /// </summary>
    /// <remarks>
    /// Selection rules:
    /// <list type="bullet">
    /// <item>When an active provider is set, its registered client is used.</item>
    /// <item>When no active provider is set but exactly one provider is registered,
    /// that single provider is used (so single-provider apps work without an explicit
    /// activation).</item>
    /// <item>Otherwise the resolution fails, because the active provider is ambiguous
    /// or unknown.</item>
    /// </list>
    /// </remarks>
    public sealed class RoutingOidcClient : IOidcClient
    {
        private readonly IReadOnlyList<IdentityProviderOidcClient> _providers;
        private readonly IActiveIdentityProvider _activeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingOidcClient"/> class.
        /// </summary>
        /// <param name="providers">The registered identity-provider clients.</param>
        /// <param name="activeProvider">The active-provider state.</param>
        public RoutingOidcClient(
            IEnumerable<IdentityProviderOidcClient> providers,
            IActiveIdentityProvider activeProvider)
        {
            ArgumentNullException.ThrowIfNull(providers);
            _providers = providers.ToList();
            _activeProvider = activeProvider ?? throw new ArgumentNullException(nameof(activeProvider));
        }

        /// <inheritdoc />
        public Task<OidcLoginResult> LoginAsync(CancellationToken cancellationToken = default) =>
            Resolve().LoginAsync(cancellationToken);

        /// <inheritdoc />
        public Task<OidcRefreshResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
            Resolve().RefreshTokenAsync(refreshToken, cancellationToken);

        /// <inheritdoc />
        public Task LogoutAsync(string? identityToken, CancellationToken cancellationToken = default) =>
            Resolve().LogoutAsync(identityToken, cancellationToken);

        private IOidcClient Resolve()
        {
            if (_providers.Count == 0)
            {
                throw new InvalidOperationException(
                    "No identity provider is registered. Register one, for example with AddIas() or the Azure Entra provider.");
            }

            var activeKey = _activeProvider.ActiveProviderKey;
            if (activeKey is not null)
            {
                foreach (var provider in _providers)
                {
                    if (string.Equals(provider.ProviderKey, activeKey, StringComparison.OrdinalIgnoreCase))
                    {
                        return provider.Client;
                    }
                }

                throw new InvalidOperationException(
                    $"No identity provider is registered for the active key '{activeKey}'.");
            }

            if (_providers.Count == 1)
            {
                return _providers[0].Client;
            }

            throw new InvalidOperationException(
                "Multiple identity providers are registered but none is active. Apply an environment or set the active provider before signing in.");
        }
    }
}

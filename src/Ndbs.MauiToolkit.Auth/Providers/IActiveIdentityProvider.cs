namespace Ndbs.MauiToolkit.Auth.Providers
{
    /// <summary>
    /// Holds the identity provider that is currently active for the app. Exactly one
    /// identity provider is active at a time; environment components set the active
    /// provider when an environment is applied, so the app can support several
    /// providers (for example IAS and Azure Entra) across environments while each
    /// environment resolves to a single provider.
    /// </summary>
    public interface IActiveIdentityProvider
    {
        /// <summary>
        /// Gets the key of the currently active identity provider, or
        /// <see langword="null"/> when no provider has been activated yet.
        /// </summary>
        string? ActiveProviderKey { get; }

        /// <summary>
        /// Sets the currently active identity provider.
        /// </summary>
        /// <param name="providerKey">The provider key to activate.</param>
        /// <exception cref="ArgumentException">When <paramref name="providerKey"/> is null or whitespace.</exception>
        void SetActiveProvider(string providerKey);
    }
}

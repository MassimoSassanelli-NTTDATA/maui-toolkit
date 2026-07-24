namespace Ndbs.MauiToolkit.Auth.Providers
{
    /// <summary>
    /// Thread-safe default implementation of <see cref="IActiveIdentityProvider"/>.
    /// Registered as a singleton so the active provider is shared app-wide.
    /// </summary>
    public sealed class ActiveIdentityProvider : IActiveIdentityProvider
    {
        private readonly object _gate = new();
        private string? _activeProviderKey;

        /// <inheritdoc />
        public string? ActiveProviderKey
        {
            get
            {
                lock (_gate)
                {
                    return _activeProviderKey;
                }
            }
        }

        /// <inheritdoc />
        public void SetActiveProvider(string providerKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);

            lock (_gate)
            {
                _activeProviderKey = providerKey;
            }
        }
    }
}

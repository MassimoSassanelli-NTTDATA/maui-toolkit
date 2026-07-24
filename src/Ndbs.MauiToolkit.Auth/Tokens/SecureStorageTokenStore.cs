using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Ndbs.MauiToolkit.Auth.Tokens
{
    /// <summary>
    /// Stores the <see cref="OidcTokenSet"/> as a single JSON document in the
    /// platform secure storage (A2). Serialization is source-generated for AOT and
    /// trimming safety (N1).
    /// </summary>
    public sealed class SecureStorageTokenStore : ITokenStore
    {
        internal const string StorageKey = "ndbs.auth.tokens";

        private readonly ISecureStorage _secureStorage;
        private readonly ILogger<SecureStorageTokenStore> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureStorageTokenStore"/> class.
        /// </summary>
        /// <param name="secureStorage">The platform secure storage abstraction.</param>
        /// <param name="logger">The logger.</param>
        public SecureStorageTokenStore(ISecureStorage secureStorage, ILogger<SecureStorageTokenStore> logger)
        {
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<OidcTokenSet?> GetAsync(CancellationToken cancellationToken = default)
        {
            var json = await _secureStorage.GetAsync(StorageKey).ConfigureAwait(false);
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize(json, AuthJsonContext.Default.OidcTokenSet);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Stored token set could not be deserialized; treating as signed out.");
                _secureStorage.Remove(StorageKey);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task SaveAsync(OidcTokenSet tokens, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tokens);

            var json = JsonSerializer.Serialize(tokens, AuthJsonContext.Default.OidcTokenSet);
            await _secureStorage.SetAsync(StorageKey, json).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            _secureStorage.Remove(StorageKey);
            return Task.CompletedTask;
        }
    }
}

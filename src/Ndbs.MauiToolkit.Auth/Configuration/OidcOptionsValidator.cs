namespace Ndbs.MauiToolkit.Auth.Configuration
{
    /// <summary>
    /// Validates <see cref="OidcOptions"/> and enforces that mandatory parameters
    /// are present and well formed (A17).
    /// </summary>
    public static class OidcOptionsValidator
    {
        /// <summary>
        /// Validates the supplied options.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="options"/> is <see langword="null"/>.</exception>
        /// <exception cref="OidcConfigurationException">
        /// When a mandatory value (Authority, ClientId, RedirectUri) is missing or invalid.
        /// </exception>
        public static void Validate(OidcOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.Authority))
                throw new OidcConfigurationException("OIDC configuration is invalid: 'Authority' is required.");

            if (!IsAbsoluteHttpUri(options.Authority))
                throw new OidcConfigurationException($"OIDC configuration is invalid: 'Authority' must be an absolute http(s) URL but was '{options.Authority}'.");

            if (string.IsNullOrWhiteSpace(options.ClientId))
                throw new OidcConfigurationException("OIDC configuration is invalid: 'ClientId' is required.");

            if (string.IsNullOrWhiteSpace(options.RedirectUri))
                throw new OidcConfigurationException("OIDC configuration is invalid: 'RedirectUri' is required.");

            if (!Uri.TryCreate(options.RedirectUri, UriKind.Absolute, out _))
                throw new OidcConfigurationException($"OIDC configuration is invalid: 'RedirectUri' must be an absolute URI but was '{options.RedirectUri}'.");

            if (options.Scopes is null || options.Scopes.Count == 0)
                throw new OidcConfigurationException("OIDC configuration is invalid: at least one scope is required.");

            if (!options.Scopes.Contains("openid"))
                throw new OidcConfigurationException("OIDC configuration is invalid: the 'openid' scope is required.");
        }

        private static bool IsAbsoluteHttpUri(string value)
            => Uri.TryCreate(value, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

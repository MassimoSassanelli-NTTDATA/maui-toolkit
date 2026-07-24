namespace Ndbs.MauiToolkit.Auth.Tokens
{
    /// <summary>
    /// Immutable set of tokens obtained from the identity provider. Persisted in
    /// secure device storage so the user stays signed in across app restarts (A2).
    /// </summary>
    public sealed class OidcTokenSet
    {
        /// <summary>
        /// Gets or sets the access token used to authorize API calls.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the refresh token used to silently renew the access token (A4).
        /// May be empty when the provider does not issue one.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identity token carrying the user's identity claims (A6).
        /// </summary>
        public string IdentityToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the absolute instant at which the access token expires.
        /// </summary>
        public DateTimeOffset AccessTokenExpiration { get; set; }

        /// <summary>
        /// Determines whether the access token is currently usable, taking a safety
        /// margin into account so it is refreshed slightly before it actually expires.
        /// </summary>
        /// <param name="now">The current instant.</param>
        /// <param name="margin">
        /// A safety margin subtracted from the expiry. Defaults to one minute.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when an access token exists and has not (nearly) expired.
        /// </returns>
        public bool IsAccessTokenValid(DateTimeOffset now, TimeSpan? margin = null)
        {
            if (string.IsNullOrEmpty(AccessToken))
                return false;

            var safety = margin ?? TimeSpan.FromMinutes(1);
            return AccessTokenExpiration - safety > now;
        }

        /// <summary>
        /// Gets a value indicating whether a refresh token is available.
        /// </summary>
        public bool HasRefreshToken => !string.IsNullOrEmpty(RefreshToken);
    }
}

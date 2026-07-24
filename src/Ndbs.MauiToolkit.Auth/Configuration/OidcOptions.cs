namespace Ndbs.MauiToolkit.Auth.Configuration
{
    /// <summary>
    /// Holds the OIDC parameters used by the library to drive the
    /// Authorization Code Flow with PKCE (A8). A single instance fully describes
    /// how the app talks to one identity provider.
    /// </summary>
    /// <remarks>
    /// PKCE is always used and no client secret is required (A16). The values can be
    /// updated at runtime through
    /// <see cref="IOidcOptionsProvider"/> (A9).
    /// </remarks>
    public sealed class OidcOptions
    {
        /// <summary>
        /// Gets or sets the issuer / authority URL of the OIDC identity provider
        /// (for example the SAP IAS tenant URL). Required.
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the public client identifier registered with the identity
        /// provider. Required.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the redirect URI the identity provider returns to after a
        /// successful sign-in. For mobile apps this is usually a custom URI scheme
        /// (for example <c>myapp://callback</c>). Required.
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URI the identity provider returns to after an
        /// RP-initiated sign-out. When not set, <see cref="RedirectUri"/> is used.
        /// </summary>
        public string? PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the OAuth scopes requested during sign-in. Defaults to the
        /// scopes required for OIDC sign-in with refresh-token support.
        /// </summary>
        public IList<string> Scopes { get; set; } = new List<string> { "openid", "profile", "email", "offline_access" };

        /// <summary>
        /// Gets or sets a value indicating whether an RP-initiated logout request is
        /// sent to the identity provider during sign-out, in addition to clearing the
        /// local token store (A5). Useful for shared devices.
        /// </summary>
        public bool EnableRpInitiatedLogout { get; set; }

        /// <summary>
        /// Gets or sets the browser experience used for the interactive sign-in (A11).
        /// On Windows the embedded web view is always used regardless of this value,
        /// because the MAUI <c>WebAuthenticator</c> (system browser) is not available
        /// there.
        /// </summary>
        public OidcBrowserKind Browser { get; set; } = OidcBrowserKind.System;

        /// <summary>
        /// Gets or sets a value indicating whether an ephemeral browser session
        /// (no shared cookies) is requested on supporting platforms such as iOS (A11).
        /// </summary>
        public bool UseEphemeralBrowserSession { get; set; }

        /// <summary>
        /// Gets the configurable claim-to-profile mappings (A6).
        /// </summary>
        public OidcClaimMappings ClaimMappings { get; set; } = new();

        /// <summary>
        /// Creates a deep copy of the current options. Used to expose immutable
        /// snapshots while the live configuration can change at runtime (A9).
        /// </summary>
        /// <returns>A new <see cref="OidcOptions"/> with identical values.</returns>
        public OidcOptions Clone() => new()
        {
            Authority = Authority,
            ClientId = ClientId,
            RedirectUri = RedirectUri,
            PostLogoutRedirectUri = PostLogoutRedirectUri,
            Scopes = new List<string>(Scopes),
            EnableRpInitiatedLogout = EnableRpInitiatedLogout,
            Browser = Browser,
            UseEphemeralBrowserSession = UseEphemeralBrowserSession,
            ClaimMappings = ClaimMappings.Clone(),
        };

        /// <summary>
        /// Gets the space-separated scope string expected by the OIDC client.
        /// </summary>
        /// <returns>The requested scopes joined by single spaces.</returns>
        public string GetScopeString() => string.Join(' ', Scopes);
    }
}

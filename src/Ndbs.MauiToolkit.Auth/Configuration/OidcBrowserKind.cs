namespace Ndbs.MauiToolkit.Auth.Configuration
{
    /// <summary>
    /// Selects which browser experience is used to present the identity provider's
    /// interactive sign-in dialog (A11).
    /// </summary>
    public enum OidcBrowserKind
    {
        /// <summary>
        /// Use the operating-system browser (Custom Tabs / SFAuthenticationSession /
        /// system browser). This is the recommended, most secure default.
        /// </summary>
        System = 0,

        /// <summary>
        /// Use an embedded web view hosted by the app. Supported on Android, iOS, and
        /// Windows. On Windows this mode is always used because the MAUI
        /// <c>WebAuthenticator</c> (system browser) is not available there.
        /// </summary>
        EmbeddedWebView = 1,
    }
}

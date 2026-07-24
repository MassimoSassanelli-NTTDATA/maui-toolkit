namespace Ndbs.MauiToolkit.Auth.Messaging
{
    /// <summary>
    /// The high-level authentication state of the app, published when it changes (A15).
    /// </summary>
    public enum AuthenticationState
    {
        /// <summary>
        /// The user is signed in and a usable token (or refresh token) is available.
        /// </summary>
        SignedIn = 0,

        /// <summary>
        /// The user is signed out and no tokens are stored.
        /// </summary>
        SignedOut = 1,

        /// <summary>
        /// The session has irrecoverably expired (for example the refresh token is no
        /// longer valid) and an interactive re-login is required.
        /// </summary>
        SessionExpired = 2,
    }
}

namespace Ndbs.MauiToolkit.Auth.Results
{
    /// <summary>
    /// Stable error categories returned by authentication operations so callers can
    /// react programmatically instead of parsing messages (A13).
    /// </summary>
    public enum AuthenticationErrorCode
    {
        /// <summary>No error; the operation succeeded.</summary>
        None = 0,

        /// <summary>The device is offline and the operation needs the internet (A10).</summary>
        Offline = 1,

        /// <summary>The user cancelled the interactive sign-in.</summary>
        Cancelled = 2,

        /// <summary>The identity provider returned an error during the protocol exchange.</summary>
        Protocol = 3,

        /// <summary>No refresh token is available, so the access token cannot be renewed.</summary>
        NoRefreshToken = 4,

        /// <summary>A received token failed validation (issuer, audience, signature, replay) (A16).</summary>
        InvalidToken = 5,

        /// <summary>An unexpected error occurred.</summary>
        Unexpected = 99,
    }
}

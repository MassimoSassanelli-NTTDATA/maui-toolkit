using Ndbs.MauiToolkit.Auth.Profile;
using Ndbs.MauiToolkit.Auth.Results;

namespace Ndbs.MauiToolkit.Auth
{
    /// <summary>
    /// The library's primary API. Drives the full sign-in, token, profile and
    /// sign-out lifecycle behind a small surface.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Performs an interactive, browser-based sign-in using the Authorization
        /// Code Flow with PKCE (A1). On success the tokens are persisted (A2) and a
        /// <see cref="Messaging.AuthenticationStateChangedMessage"/> is published (A15).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the sign-in (A13).</returns>
        Task<AuthenticationResult> LoginAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a valid access token, silently refreshing it via the refresh token
        /// when it has expired (A3, A4). Concurrent callers share a single refresh.
        /// </summary>
        /// <param name="forceRefresh">When <see langword="true"/>, refreshes even if the current token still appears valid.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>
        /// A valid access token, or <see langword="null"/> when none can be obtained
        /// (for example offline or the session has expired).
        /// </returns>
        Task<string?> GetAccessTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs the user out by clearing the local token store and, when requested or
        /// configured, performing an RP-initiated logout at the identity provider (A5).
        /// Publishes a <see cref="Messaging.AuthenticationStateChangedMessage"/> (A15).
        /// </summary>
        /// <param name="remoteLogout">When <see langword="true"/>, also signs out at the identity provider.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogoutAsync(bool remoteLogout = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indicates whether a user is currently signed in, i.e. a usable access token
        /// or a refresh token is available (A7).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><see langword="true"/> when the user is signed in.</returns>
        Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the signed-in user's profile derived from the identity token claims
        /// using the configured claim mappings (A6).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The user profile, or <see langword="null"/> when no user is signed in.</returns>
        Task<UserProfile?> GetUserProfileAsync(CancellationToken cancellationToken = default);
    }
}

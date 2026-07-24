namespace Ndbs.MauiToolkit.Auth.Client
{
    /// <summary>
    /// Abstraction over the OIDC protocol operations (login, refresh, logout).
    /// Keeping the concrete <c>Duende.IdentityModel.OidcClient</c> usage behind this
    /// interface keeps the orchestration in
    /// <see cref="Ndbs.MauiToolkit.Auth.IAuthenticationService"/> testable.
    /// </summary>
    public interface IOidcClient
    {
        /// <summary>
        /// Performs an interactive sign-in via the platform browser using the
        /// Authorization Code Flow with PKCE (A1, A16).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The login result, including tokens and identity claims.</returns>
        Task<OidcLoginResult> LoginAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Renews the access token using a refresh token (A4).
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The refresh result, including the renewed tokens.</returns>
        Task<OidcRefreshResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs an RP-initiated sign-out at the identity provider (A5).
        /// </summary>
        /// <param name="identityToken">The identity token used as the logout hint, if any.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogoutAsync(string? identityToken, CancellationToken cancellationToken = default);
    }
}

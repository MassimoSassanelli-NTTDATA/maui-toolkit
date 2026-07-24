namespace Ndbs.MauiToolkit.Auth.Entra
{
    /// <summary>
    /// Thin, provider-neutral seam over the MSAL public-client operations the toolkit
    /// needs. The real implementation (<see cref="Msal.MsalPublicClientAdapter"/>)
    /// wraps <c>Microsoft.Identity.Client</c>; keeping it behind this interface makes
    /// the token mapping in <see cref="MsalEntraClient"/> unit-testable.
    /// </summary>
    public interface IMsalPublicClient
    {
        /// <summary>
        /// Acquires a token through an interactive, browser-based sign-in.
        /// </summary>
        /// <param name="scopes">The resource scopes to request (reserved OIDC scopes excluded).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The MSAL authentication result.</returns>
        /// <exception cref="MsalAuthenticationException">When the sign-in fails (for example is cancelled).</exception>
        Task<MsalAuthResult> AcquireTokenInteractiveAsync(IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Acquires a token silently from the MSAL token cache.
        /// </summary>
        /// <param name="scopes">The resource scopes to request.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>
        /// The MSAL authentication result, or <see langword="null"/> when no cached
        /// account is available and interactive sign-in is required.
        /// </returns>
        Task<MsalAuthResult?> AcquireTokenSilentAsync(IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs the user out by removing the cached account(s).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task SignOutAsync(CancellationToken cancellationToken = default);
    }
}

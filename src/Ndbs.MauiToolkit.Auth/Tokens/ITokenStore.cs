namespace Ndbs.MauiToolkit.Auth.Tokens
{
    /// <summary>
    /// Persists and retrieves the current <see cref="OidcTokenSet"/> in secure
    /// device storage (A2, A16).
    /// </summary>
    public interface ITokenStore
    {
        /// <summary>
        /// Loads the persisted token set.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The stored token set, or <see langword="null"/> when none is stored.</returns>
        Task<OidcTokenSet?> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists the supplied token set, replacing any previously stored value.
        /// </summary>
        /// <param name="tokens">The tokens to persist.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveAsync(OidcTokenSet tokens, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes any persisted token set (used during sign-out, A5).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}

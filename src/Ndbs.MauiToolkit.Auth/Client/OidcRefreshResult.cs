using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tokens;

namespace Ndbs.MauiToolkit.Auth.Client
{
    /// <summary>
    /// Result of a token refresh carried out by an <see cref="IOidcClient"/> (A4).
    /// </summary>
    public sealed class OidcRefreshResult
    {
        private OidcRefreshResult(bool isSuccess, AuthenticationErrorCode errorCode, string? errorMessage, OidcTokenSet? tokens)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Tokens = tokens;
        }

        /// <summary>Gets a value indicating whether the refresh succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>Gets the error category when the refresh failed.</summary>
        public AuthenticationErrorCode ErrorCode { get; }

        /// <summary>Gets a human-readable error message when the refresh failed.</summary>
        public string? ErrorMessage { get; }

        /// <summary>Gets the renewed tokens when the refresh succeeded.</summary>
        public OidcTokenSet? Tokens { get; }

        /// <summary>Creates a successful refresh result.</summary>
        /// <param name="tokens">The renewed tokens.</param>
        /// <returns>A successful <see cref="OidcRefreshResult"/>.</returns>
        public static OidcRefreshResult Success(OidcTokenSet tokens)
            => new(true, AuthenticationErrorCode.None, null, tokens);

        /// <summary>Creates a failed refresh result.</summary>
        /// <param name="errorCode">The error category.</param>
        /// <param name="errorMessage">A human-readable error message.</param>
        /// <returns>A failed <see cref="OidcRefreshResult"/>.</returns>
        public static OidcRefreshResult Failure(AuthenticationErrorCode errorCode, string errorMessage)
            => new(false, errorCode, errorMessage, null);
    }
}

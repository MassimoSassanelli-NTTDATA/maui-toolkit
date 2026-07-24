using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tokens;

namespace Ndbs.MauiToolkit.Auth.Client
{
    /// <summary>
    /// Result of an interactive sign-in carried out by an <see cref="IOidcClient"/>.
    /// </summary>
    public sealed class OidcLoginResult
    {
        private OidcLoginResult(bool isSuccess, AuthenticationErrorCode errorCode, string? errorMessage, OidcTokenSet? tokens, IReadOnlyList<KeyValuePair<string, string>> claims)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Tokens = tokens;
            Claims = claims;
        }

        /// <summary>Gets a value indicating whether the sign-in succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>Gets the error category when the sign-in failed.</summary>
        public AuthenticationErrorCode ErrorCode { get; }

        /// <summary>Gets a human-readable error message when the sign-in failed.</summary>
        public string? ErrorMessage { get; }

        /// <summary>Gets the obtained tokens when the sign-in succeeded.</summary>
        public OidcTokenSet? Tokens { get; }

        /// <summary>Gets the identity-token claims used to build the user profile (A6).</summary>
        public IReadOnlyList<KeyValuePair<string, string>> Claims { get; }

        /// <summary>Creates a successful login result.</summary>
        /// <param name="tokens">The obtained tokens.</param>
        /// <param name="claims">The identity claims.</param>
        /// <returns>A successful <see cref="OidcLoginResult"/>.</returns>
        public static OidcLoginResult Success(OidcTokenSet tokens, IReadOnlyList<KeyValuePair<string, string>> claims)
            => new(true, AuthenticationErrorCode.None, null, tokens, claims);

        /// <summary>Creates a failed login result.</summary>
        /// <param name="errorCode">The error category.</param>
        /// <param name="errorMessage">A human-readable error message.</param>
        /// <returns>A failed <see cref="OidcLoginResult"/>.</returns>
        public static OidcLoginResult Failure(AuthenticationErrorCode errorCode, string errorMessage)
            => new(false, errorCode, errorMessage, null, Array.Empty<KeyValuePair<string, string>>());
    }
}

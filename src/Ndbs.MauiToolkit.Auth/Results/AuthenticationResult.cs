using Ndbs.MauiToolkit.Auth.Profile;

namespace Ndbs.MauiToolkit.Auth.Results
{
    /// <summary>
    /// Outcome of an authentication operation. Errors are surfaced as data rather
    /// than being allowed to propagate uncontrolled (A13).
    /// </summary>
    public sealed class AuthenticationResult
    {
        private AuthenticationResult(bool isSuccess, AuthenticationErrorCode errorCode, string? errorMessage, UserProfile? userProfile)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            UserProfile = userProfile;
        }

        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the error category when the operation failed (A13).
        /// </summary>
        public AuthenticationErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets a human-readable error message when the operation failed.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Gets the signed-in user's profile when the operation succeeded (A6).
        /// </summary>
        public UserProfile? UserProfile { get; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="userProfile">The signed-in user's profile.</param>
        /// <returns>A successful <see cref="AuthenticationResult"/>.</returns>
        public static AuthenticationResult Success(UserProfile userProfile)
            => new(true, AuthenticationErrorCode.None, null, userProfile);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="errorCode">The error category.</param>
        /// <param name="errorMessage">A human-readable error message.</param>
        /// <returns>A failed <see cref="AuthenticationResult"/>.</returns>
        public static AuthenticationResult Failure(AuthenticationErrorCode errorCode, string errorMessage)
            => new(false, errorCode, errorMessage, null);
    }
}

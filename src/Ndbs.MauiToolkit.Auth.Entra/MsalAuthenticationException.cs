using Ndbs.MauiToolkit.Auth.Results;

namespace Ndbs.MauiToolkit.Auth.Entra
{
    /// <summary>
    /// Provider-neutral exception raised by an <see cref="IMsalPublicClient"/> when an
    /// MSAL operation fails in a way that maps to a stable
    /// <see cref="AuthenticationErrorCode"/> (for example a cancelled interactive
    /// sign-in). Keeps MSAL exception types out of <see cref="MsalEntraClient"/>.
    /// </summary>
    public sealed class MsalAuthenticationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsalAuthenticationException"/> class.
        /// </summary>
        /// <param name="errorCode">The stable error category.</param>
        /// <param name="message">A human-readable error message.</param>
        public MsalAuthenticationException(AuthenticationErrorCode errorCode, string message)
            : base(message) => ErrorCode = errorCode;

        /// <summary>Gets the stable error category for this failure.</summary>
        public AuthenticationErrorCode ErrorCode { get; }
    }
}

namespace Ndbs.MauiToolkit.Auth.Configuration
{
    /// <summary>
    /// Thrown when the OIDC configuration is missing required values or contains
    /// invalid values. Raised early — during service registration or start — so
    /// configuration mistakes surface before the first sign-in attempt (A17).
    /// </summary>
    public sealed class OidcConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OidcConfigurationException"/> class.
        /// </summary>
        /// <param name="message">A description of the configuration problem.</param>
        public OidcConfigurationException(string message)
            : base(message)
        {
        }
    }
}

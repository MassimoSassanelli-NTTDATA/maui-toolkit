namespace Ndbs.MauiToolkit.Auth.Configuration
{
    /// <summary>
    /// Provides access to the current <see cref="OidcOptions"/> and allows the
    /// configuration to be changed at runtime (A9), for example to switch identity
    /// provider after scanning a QR code.
    /// </summary>
    public interface IOidcOptionsProvider
    {
        /// <summary>
        /// Gets an immutable snapshot of the current options.
        /// </summary>
        /// <returns>A copy of the current <see cref="OidcOptions"/>.</returns>
        OidcOptions Current { get; }

        /// <summary>
        /// Raised whenever the configuration changes so dependent components can
        /// rebuild their state (for example the OIDC client).
        /// </summary>
        event EventHandler? OptionsChanged;

        /// <summary>
        /// Replaces the entire configuration. The new options are validated before
        /// being applied (A17).
        /// </summary>
        /// <param name="options">The replacement options.</param>
        void Replace(OidcOptions options);

        /// <summary>
        /// Applies a partial update to the current configuration. The mutation runs
        /// against a copy that is validated before being applied (A9, A17).
        /// </summary>
        /// <param name="update">An action that mutates the working copy.</param>
        void Update(Action<OidcOptions> update);
    }
}

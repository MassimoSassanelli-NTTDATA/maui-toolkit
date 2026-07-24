using Duende.IdentityModel.OidcClient.Browser;
using Ndbs.MauiToolkit.Auth.Configuration;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace Ndbs.MauiToolkit.Auth.Ias
{
    /// <summary>
    /// <see cref="IBrowser"/> that selects the concrete browser used for the
    /// interactive sign-in based on the configured <see cref="OidcBrowserKind"/> and
    /// the current platform (A11). On Windows the embedded
    /// <see cref="MauiWebViewBrowser"/> is always used because the MAUI
    /// <c>WebAuthenticator</c> (system browser) is not available there. On the other
    /// platforms the <see cref="OidcOptions.Browser"/> setting decides between the
    /// system browser and the embedded web view, defaulting to the system browser.
    /// </summary>
    public sealed class MauiPlatformBrowser : IBrowser
    {
        private readonly IOidcOptionsProvider _optionsProvider;
        private readonly MauiAuthenticatorBrowser _systemBrowser;
        private readonly MauiWebViewBrowser _embeddedBrowser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiPlatformBrowser"/> class.
        /// </summary>
        /// <param name="optionsProvider">The runtime OIDC configuration provider (A11).</param>
        /// <param name="systemBrowser">The system-browser implementation.</param>
        /// <param name="embeddedBrowser">The embedded web-view implementation.</param>
        public MauiPlatformBrowser(
            IOidcOptionsProvider optionsProvider,
            MauiAuthenticatorBrowser systemBrowser,
            MauiWebViewBrowser embeddedBrowser)
        {
            _optionsProvider = optionsProvider ?? throw new ArgumentNullException(nameof(optionsProvider));
            _systemBrowser = systemBrowser ?? throw new ArgumentNullException(nameof(systemBrowser));
            _embeddedBrowser = embeddedBrowser ?? throw new ArgumentNullException(nameof(embeddedBrowser));
        }

        /// <inheritdoc />
        public Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
            => SelectBrowser().InvokeAsync(options, cancellationToken);

        private IBrowser SelectBrowser()
        {
#if WINDOWS
            // MAUI WebAuthenticator is not implemented on Windows; the embedded
            // web view is the only supported interactive browser there.
            return _embeddedBrowser;
#else
            return _optionsProvider.Current.Browser == OidcBrowserKind.EmbeddedWebView
                ? _embeddedBrowser
                : _systemBrowser;
#endif
        }
    }
}

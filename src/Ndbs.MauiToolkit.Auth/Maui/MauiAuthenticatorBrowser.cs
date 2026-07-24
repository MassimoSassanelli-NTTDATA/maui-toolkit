using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Authentication;
using Ndbs.MauiToolkit.Auth.Configuration;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace Ndbs.MauiToolkit.Auth.Maui
{
    /// <summary>
    /// Duende <see cref="IBrowser"/> implementation that drives the interactive
    /// sign-in/sign-out through the MAUI <see cref="WebAuthenticator"/>. The
    /// authenticator uses the platform system browser and routes the callback back
    /// to the running app instance via the registered custom URI scheme (A11, A12).
    /// </summary>
    public sealed class MauiAuthenticatorBrowser : IBrowser
    {
        private readonly IOidcOptionsProvider _optionsProvider;
        private readonly ILogger<MauiAuthenticatorBrowser> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiAuthenticatorBrowser"/> class.
        /// </summary>
        /// <param name="optionsProvider">The OIDC configuration provider (browser options, A11).</param>
        /// <param name="logger">The logger (A13).</param>
        public MauiAuthenticatorBrowser(IOidcOptionsProvider optionsProvider, ILogger<MauiAuthenticatorBrowser> logger)
        {
            _optionsProvider = optionsProvider ?? throw new ArgumentNullException(nameof(optionsProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            try
            {
                var authenticatorOptions = new WebAuthenticatorOptions
                {
                    Url = new Uri(options.StartUrl),
                    CallbackUrl = new Uri(options.EndUrl),
                    PrefersEphemeralWebBrowserSession = _optionsProvider.Current.UseEphemeralBrowserSession,
                };

                var result = await WebAuthenticator.Default.AuthenticateAsync(authenticatorOptions).ConfigureAwait(false);

                var responseUrl = new RequestUrl(options.EndUrl).Create(new Parameters(result.Properties));
                return new BrowserResult
                {
                    Response = responseUrl,
                    ResultType = BrowserResultType.Success,
                };
            }
            catch (TaskCanceledException)
            {
                return new BrowserResult { ResultType = BrowserResultType.UserCancel };
            }
            catch (OperationCanceledException)
            {
                return new BrowserResult { ResultType = BrowserResultType.UserCancel };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The interactive browser session failed.");
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = ex.Message,
                };
            }
        }
    }
}

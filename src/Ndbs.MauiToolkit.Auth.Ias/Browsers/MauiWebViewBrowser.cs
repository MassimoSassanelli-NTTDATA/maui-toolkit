using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace Ndbs.MauiToolkit.Auth.Ias
{
    /// <summary>
    /// Embedded-browser <see cref="IBrowser"/> implementation that hosts the identity
    /// provider's interactive sign-in/sign-out dialog inside an in-app
    /// <see cref="WebView"/> presented as a modal page (A11). The redirect to the
    /// configured callback URI is intercepted in-process via the
    /// <see cref="WebView.Navigating"/> event, so no operating-system browser and no
    /// OS-registered URI scheme are required; a custom-scheme redirect URI works on
    /// every platform. This is the only supported mode on Windows, where the MAUI
    /// <c>WebAuthenticator</c> (system browser) is unavailable.
    /// </summary>
    public sealed class MauiWebViewBrowser : IBrowser
    {
        private readonly ILogger<MauiWebViewBrowser> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiWebViewBrowser"/> class.
        /// </summary>
        /// <param name="logger">The logger (A13).</param>
        public MauiWebViewBrowser(ILogger<MauiWebViewBrowser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            try
            {
                return await MainThread.InvokeOnMainThreadAsync(() => RunAsync(options, cancellationToken)).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return new BrowserResult { ResultType = BrowserResultType.UserCancel };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The embedded browser session failed.");
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = ex.Message,
                };
            }
        }

        private static async Task<BrowserResult> RunAsync(BrowserOptions options, CancellationToken cancellationToken)
        {
            var navigation = GetNavigation();
            var completion = new TaskCompletionSource<BrowserResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            var webView = new WebView
            {
                Source = options.StartUrl,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
            };

            var page = new ContentPage
            {
                Title = "Sign in",
                Content = webView,
            };

            void OnNavigating(object? sender, WebNavigatingEventArgs e)
            {
                if (e.Url.StartsWith(options.EndUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // Intercept the callback in-process instead of letting the WebView
                    // navigate to the (often custom-scheme) redirect URI.
                    e.Cancel = true;
                    completion.TrySetResult(new BrowserResult
                    {
                        Response = e.Url,
                        ResultType = BrowserResultType.Success,
                    });
                }
            }

            // A manual dismissal of the modal counts as a user cancellation.
            void OnDisappearing(object? sender, EventArgs e)
                => completion.TrySetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel });

            webView.Navigating += OnNavigating;
            page.Disappearing += OnDisappearing;

            using var registration = cancellationToken.Register(
                () => completion.TrySetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel }));

            await navigation.PushModalAsync(page).ConfigureAwait(true);

            try
            {
                return await completion.Task.ConfigureAwait(true);
            }
            finally
            {
                webView.Navigating -= OnNavigating;
                page.Disappearing -= OnDisappearing;

                // Only pop when the page is still on the modal stack; a manual
                // dismissal removes it already.
                if (navigation.ModalStack.Contains(page))
                    await navigation.PopModalAsync().ConfigureAwait(true);
            }
        }

        private static INavigation GetNavigation()
        {
            var application = Application.Current
                ?? throw new InvalidOperationException("No current MAUI application is available to host the embedded browser.");

            var page = application.Windows.FirstOrDefault()?.Page
                ?? throw new InvalidOperationException("No active page is available in the first application window to host the embedded browser.");

            return page.Navigation;
        }
    }
}

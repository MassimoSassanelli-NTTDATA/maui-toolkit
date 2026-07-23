using Microsoft.Extensions.Logging;

namespace Ndbs.MauiToolkit.Services
{
    /// <summary>
    /// Provides navigation services for the application using .NET MAUI Shell navigation.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly ILogger<NavigationService> _logger;

        #region Ctor.

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging navigation events.</param>
        public NavigationService(ILogger<NavigationService> logger)
        {
            _logger = logger;
        }

        #endregion

        public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            try
            {
                _logger.LogTrace("Navigating to route: {Route}", route);

                // Shell navigation touches native UI objects and must run on the UI
                // thread. Callers may await us from a background thread (e.g. after a
                // ConfigureAwait(false) continuation), so marshal explicitly to avoid
                // RPC_E_WRONG_THREAD (0x8001010E) on Windows/WinUI.
                await MainThread.InvokeOnMainThreadAsync(() =>
                    parameters != null
                        ? Shell.Current.GoToAsync(route, parameters)
                        : Shell.Current.GoToAsync(route));

                _logger.LogTrace("Successfully navigated to route: {Route}", route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to navigate to route: {Route}", route);
                throw;
            }
        }

        public async Task GoBackAsync(IDictionary<string, object>? parameters = null)
        {
            try
            {
                _logger.LogTrace("Navigating back to the previous page.");

                // Marshal to the UI thread; see NavigateToAsync for rationale.
                await MainThread.InvokeOnMainThreadAsync(() =>
                    parameters == null
                        ? Shell.Current.GoToAsync("..")
                        : Shell.Current.GoToAsync("..", parameters));

                _logger.LogTrace("Successfully navigated back to the previous page.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to navigate back to the previous page.");
                throw;
            }
        }
    }
}

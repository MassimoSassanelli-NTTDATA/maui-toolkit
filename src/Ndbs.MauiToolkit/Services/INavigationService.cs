namespace Ndbs.MauiToolkit.Services
{
    /// <summary>
    /// Defines the contract for navigation services in the application.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a specified route with optional parameters.
        /// </summary>
        /// <param name="route">The route to navigate to.</param>
        /// <param name="parameters">Optional parameters to pass to the target route.</param>
        /// <returns>A task that represents the asynchronous navigation operation.</returns>
        Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);

        /// <summary>
        /// Navigates back to the previous page in the navigation stack.
        /// If parameters are provided they are delivered to the target page (e.g. its BindingContext) before it appears.
        /// </summary>
        /// <param name="parameters">
        /// Optional key/value pairs passed to the page being navigated back to. Keys should be unique; values may be null.
        /// </param>
        /// <returns>A task representing the asynchronous navigation operation.</returns>
        Task GoBackAsync(IDictionary<string, object>? parameters = null);
    }
}

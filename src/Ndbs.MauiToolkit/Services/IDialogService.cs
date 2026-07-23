namespace Ndbs.MauiToolkit.Services
{
    /// <summary>
    /// Defines an abstraction for presenting simple modal dialogs (alerts and confirmations)
    /// so that view models can request user confirmation without depending on UI types.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Displays a confirmation dialog with an accept and a cancel button.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="message">The dialog message.</param>
        /// <param name="accept">The text of the accept button.</param>
        /// <param name="cancel">The text of the cancel button.</param>
        /// <returns>
        /// A task that resolves to <see langword="true"/> when the user accepts; otherwise <see langword="false"/>.
        /// </returns>
        Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);

        /// <summary>
        /// Displays an informational alert with a single dismiss button.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="message">The dialog message.</param>
        /// <param name="cancel">The text of the dismiss button.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AlertAsync(string title, string message, string cancel);
    }
}

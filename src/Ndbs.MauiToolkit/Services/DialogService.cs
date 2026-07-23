namespace Ndbs.MauiToolkit.Services
{
    /// <summary>
    /// MAUI implementation of <see cref="IDialogService"/> that presents dialogs through the
    /// current application window's page.
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <inheritdoc />
        public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
        {
            var page = GetCurrentPage();
            return page is null
                ? Task.FromResult(false)
                : page.DisplayAlert(title, message, accept, cancel);
        }

        /// <inheritdoc />
        public Task AlertAsync(string title, string message, string cancel)
        {
            var page = GetCurrentPage();
            return page is null
                ? Task.CompletedTask
                : page.DisplayAlert(title, message, cancel);
        }

        private static Page? GetCurrentPage()
        {
            return Application.Current?.Windows.FirstOrDefault()?.Page;
        }
    }
}

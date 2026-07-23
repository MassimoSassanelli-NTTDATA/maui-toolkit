namespace Ndbs.MauiToolkit.Mvvm
{
    public abstract class ShellBase<TViewModel> : ShellBase
        where TViewModel : BaseViewModel
    {
        protected ShellBase(TViewModel viewModel) : base(viewModel) { }

        public new TViewModel BindingContext => (TViewModel) base.BindingContext;
    }

    /// <summary>
    /// The ShellBase class provides a base implementation for the application <see cref="Shell"/>,
    /// ensuring that the InitializeAsyncCommand of the <see cref="IInitializableViewModel"/> is executed when the shell appears.
    /// </summary>
    /// <remarks>
    /// This is the <see cref="Shell"/> counterpart to <see cref="PageBase"/>. Because <see cref="Shell"/>
    /// and <see cref="ContentPage"/> are sibling <see cref="Page"/> types, the behavior cannot be inherited
    /// from <see cref="PageBase"/> and is provided here instead.
    /// <para>
    /// Two deliberate deviations from <see cref="PageBase"/>:
    /// <list type="number">
    ///   <item>
    ///     Initialization runs at most once. A shell root typically appears a single time, but some
    ///     platforms re-raise <c>OnAppearing</c> on window recreation. The <see cref="IInitializableViewModel.IsInitialized"/>
    ///     guard prevents duplicate initialization (e.g. duplicated message registrations or flyout rebuilds).
    ///   </item>
    ///   <item>
    ///     No default <c>Title</c> is assigned. Unlike a page, <see cref="Shell.Title"/> represents the
    ///     application / flyout title and must not be overwritten with the type name.
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// The sequence of method calls on the ViewModel is as follows:
    /// 1. Constructor of the ViewModel.
    /// 2. InitializeAsync (executed within InitializeAsyncCommand).
    /// </para>
    /// </remarks>
    public abstract class ShellBase : Shell
    {
        protected ShellBase(object? viewModel = null)
        {
            BindingContext = viewModel;
        }

        /// <summary>
        /// Called when the shell appears.
        /// Executes the InitializeAsyncCommand of the ViewModel if the BindingContext implements
        /// <see cref="IInitializableViewModel"/> and has not been initialized yet.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await InitializeViewModelAsync();
        }

        /// <summary>
        /// Executes the ViewModel's InitializeAsyncCommand once when the BindingContext implements
        /// <see cref="IInitializableViewModel"/> and has not been initialized yet; otherwise does nothing.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected Task InitializeViewModelAsync()
        {
            if (BindingContext is IInitializableViewModel { IsInitialized: false } viewModel)
                return viewModel.InitializeAsyncCommand.ExecuteAsync(null);

            return Task.CompletedTask;
        }
    }
}

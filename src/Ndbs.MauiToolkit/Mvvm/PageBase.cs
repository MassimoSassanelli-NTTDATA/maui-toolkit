namespace Ndbs.MauiToolkit.Mvvm
{
	public abstract class PageBase<TViewModel> : PageBase
		where TViewModel : BaseViewModel
	{
		protected PageBase(TViewModel viewModel) : base(viewModel) { }
		
		public new TViewModel BindingContext => (TViewModel) base.BindingContext;
	}
		
	/// <summary>
	/// The ContentPageBase class provides a base implementation for content pages,
	/// ensuring that the InitializeAsyncCommand of the <see cref="IInitializableViewModel"/> is executed when the page appears.
	/// </summary>
	/// <remarks>
	/// The sequence of method calls on the ViewModel is as follows:
	/// 1. Constructor of the ViewModel.
	/// 2. ApplyQueryAttributes (if the ViewModel receives navigation parameters).
	/// 3. InitializeAsync (executed within InitializeAsyncCommand).
	/// </remarks>
	public abstract class PageBase : ContentPage
	{
		protected PageBase(object? viewModel = null)
		{
			BindingContext = viewModel;

			if (string.IsNullOrWhiteSpace(Title))
			{
				Title = GetType().Name;
			}
		}

		/// <summary>
		/// Called when the page appears.
		/// Executes the InitializeAsyncCommand of the ViewModel if the BindingContext implements <see cref="IInitializableViewModel"/>.
		/// </summary>
		/// <remarks>
		/// ApplyQueryAttributes (if the ViewModel receives navigation parameters) is called before this method.
		/// The command is dispatched to the next UI cycle so that any Shell navigation triggered by
		/// <see cref="IInitializableViewModel.InitializeAsync"/> runs after the Shell has finished
		/// processing its current navigation request. Without this deferral, a GoToAsync call
		/// issued from InitializeAsync can arrive while the Shell's RequestNavigation is still in
		/// progress and throw "Pending Navigations still processing".
		/// </remarks>
		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (BindingContext is IInitializableViewModel ivmb)
				Dispatcher.Dispatch(async () => await ivmb.InitializeAsyncCommand.ExecuteAsync(null));
		}
	}
}
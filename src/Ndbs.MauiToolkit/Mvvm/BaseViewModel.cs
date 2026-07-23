using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ndbs.MauiToolkit.Mvvm
{
	/// <summary>
	/// The ViewModelBase class provides a base implementation for view models,
	/// including support for initialization and busy state management.
	/// </summary>
	/// <remarks>
	/// The sequence of method calls on the ViewModel is as follows:
	/// 1. Constructor of the ViewModel.
	/// 2. ApplyQueryAttributes (if the ViewModel receives navigation parameters).
	/// 3. InitializeAsync (executed within InitializeAsyncCommand).
	/// </remarks>
	public abstract partial class BaseViewModel : ObservableObject, IInitializableViewModel, IQueryAttributable
	{
		private long _isBusy;

		[ObservableProperty]
		private bool _isInitialized;

		#region Ctor.

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseViewModel"/> class.
		/// </summary>
		protected BaseViewModel()
		{
			InitializeAsyncCommand = new AsyncRelayCommand(
				async () =>
				{
					await IsBusyFor(InitializeAsync);
					IsInitialized = true;
				},
				AsyncRelayCommandOptions.FlowExceptionsToTaskScheduler);
		}

		#endregion

		/// <summary>
		/// Gets a value indicating whether the view model is currently busy.
		/// </summary>
		public bool IsBusy => Interlocked.Read(ref _isBusy) > 0;

		/// <summary>
		/// Gets the command to initialize the view model asynchronously.
		/// </summary>
		public IAsyncRelayCommand InitializeAsyncCommand { get; }

		
		/// <summary>
		/// Applies query attributes to the view model.
		/// </summary>
		/// <param name="query">The query attributes.</param>
		public virtual void ApplyQueryAttributes(IDictionary<string, object> query) { }

		/// <summary>
		/// Initializes the view model asynchronously.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public virtual Task InitializeAsync() => Task.CompletedTask;

		/// <summary>
		/// Executes a unit of work while indicating that the view model is busy.
		/// </summary>
		/// <param name="unitOfWork">The unit of work to execute.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		protected async Task IsBusyFor(Func<Task> unitOfWork)
		{
			Interlocked.Increment(ref _isBusy);
			OnPropertyChanged(nameof(IsBusy));
			OnIsBusyChanged(IsBusy);

			try
			{
				await unitOfWork();
			}
			finally
			{
				Interlocked.Decrement(ref _isBusy);
				OnPropertyChanged(nameof(IsBusy));
				OnIsBusyChanged(IsBusy);
			}
		}

		/// <summary>
		/// Called when the value of the IsBusy property changes.
		/// </summary>
		/// <remarks>Override this method to perform custom actions when the IsBusy state changes.</remarks>
		/// <param name="value">The new value of the IsBusy property. <see langword="true"/> if the object is now busy; otherwise, <see
		/// langword="false"/>.</param>
		protected virtual void OnIsBusyChanged(bool value)
		{
		}
	}
}
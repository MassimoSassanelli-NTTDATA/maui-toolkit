using CommunityToolkit.Mvvm.Input;

namespace Ndbs.MauiToolkit.Mvvm
{
	/// <summary>
	/// Defines a contract for view models that support asynchronous initialization.
	/// </summary>
	/// <remarks>Implement this interface to provide a standardized way to initialize view models asynchronously,
	/// typically when loading data or preparing state before use in a UI. The interface exposes an asynchronous
	/// initialization method, a command suitable for data binding, and a property indicating whether initialization has
	/// completed.</remarks>
	public interface IInitializableViewModel
	{
		public IAsyncRelayCommand InitializeAsyncCommand { get; }

		public bool IsInitialized { get; }

		Task InitializeAsync();
	}
}

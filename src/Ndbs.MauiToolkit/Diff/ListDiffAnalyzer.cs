namespace Ndbs.MauiToolkit.Diff
{
	/// <summary>
	/// Analyzes the differences between two lists of objects of type <typeparamref name="T"/>,
	/// identifying added, removed, changed, and unchanged items based on provided selectors.
	/// </summary>
	/// <typeparam name="T">The type of objects in the lists to compare.</typeparam>
	public class ListDiffAnalyzer<T>
	{
		private readonly Func<T, string> _idSelector;
		private readonly Func<T, T, bool> _isChangedFunc;

		/// <summary>
		/// Initializes a new instance of the <see cref="ListDiffAnalyzer{T}"/> class.
		/// </summary>
		/// <param name="idSelector">A function to select a unique identifier from an object of type <typeparamref name="T"/>.</param>
		/// <param name="isChangedFunc">A function that takes a local and remote object and returns true if they are considered changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="idSelector"/> or <paramref name="isChangedFunc"/> is null.</exception>
		public ListDiffAnalyzer(Func<T, string> idSelector, Func<T, T, bool> isChangedFunc)
		{
			_idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
			_isChangedFunc = isChangedFunc ?? throw new ArgumentNullException(nameof(isChangedFunc));
		}

		/// <summary>
		/// Calculates the differences between the remote and local lists.
		/// </summary>
		/// <param name="remoteList">The list representing the remote or new state.</param>
		/// <param name="localList">The list representing the local or old state.</param>
		/// <param name="cancellationToken">A token used to cancel the calculation.</param>
		/// <returns>
		/// A list of <see cref="DiffDescription{T}"/> objects describing the differences:
		/// added, removed, changed, or unchanged items.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="remoteList"/> or <paramref name="localList"/> is null.</exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown if the identifier selector returns a null identifier, or if either list contains
		/// duplicate identifiers. Identifiers must be non-null and unique within each list.
		/// </exception>
		/// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken"/> is cancelled.</exception>
		public IList<DiffDescription<T>> Calculate(IEnumerable<T> remoteList, IEnumerable<T> localList, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(remoteList);
			ArgumentNullException.ThrowIfNull(localList);

			var result = new List<DiffDescription<T>>();

			var localDict = BuildDictionary(localList, nameof(localList), cancellationToken);
			var remoteDict = BuildDictionary(remoteList, nameof(remoteList), cancellationToken);

			// Check for removed and changed/unchanged
			foreach (var local in localDict)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (!remoteDict.TryGetValue(local.Key, out var remote))
				{
					// Removed
					result.Add(new DiffDescription<T>
					{
						Type = DiffTypes.Removed,
						Local = local.Value,
						Remote = default
					});
				}
				else
				{
					if (_isChangedFunc(local.Value, remote))
					{
						result.Add(new DiffDescription<T>
						{
							Type = DiffTypes.Changed,
							Local = local.Value,
							Remote = remote
						});
					}
					else
					{
						result.Add(new DiffDescription<T>
						{
							Type = DiffTypes.Unchanged,
							Local = local.Value,
							Remote = remote
						});
					}
				}
			}

			// Check for added
			foreach (var remote in remoteDict)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (!localDict.ContainsKey(remote.Key))
				{
					result.Add(new DiffDescription<T>
					{
						Type = DiffTypes.Added,
						Local = default,
						Remote = remote.Value
					});
				}
			}

			return result;
		}

		/// <summary>
		/// Builds a lookup keyed by identifier, rejecting null and duplicate identifiers with a
		/// descriptive <see cref="InvalidOperationException"/> instead of relying on the raw
		/// <c>ToDictionary</c> failure.
		/// </summary>
		private Dictionary<string, T> BuildDictionary(IEnumerable<T> items, string listName, CancellationToken cancellationToken)
		{
			var dictionary = new Dictionary<string, T>();

			foreach (var item in items)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var id = _idSelector(item);
				if (id is null)
				{
					throw new InvalidOperationException(
						$"The identifier selector returned a null identifier for an item in '{listName}'. Identifiers must be non-null.");
				}

				if (!dictionary.TryAdd(id, item))
				{
					throw new InvalidOperationException(
						$"The list '{listName}' contains a duplicate identifier '{id}'. Identifiers must be unique within each list.");
				}
			}

			return dictionary;
		}
	}
}

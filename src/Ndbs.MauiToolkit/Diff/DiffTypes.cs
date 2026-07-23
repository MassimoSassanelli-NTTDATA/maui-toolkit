namespace Ndbs.MauiToolkit.Diff
{
	public enum DiffTypes
	{
		Added,      // Exists in remote, not in local
		Removed,    // Exists in local, not in remote
		Changed,    // Exists in both, but property differs
		Unchanged   // Exists in both, property is equal
	}
}

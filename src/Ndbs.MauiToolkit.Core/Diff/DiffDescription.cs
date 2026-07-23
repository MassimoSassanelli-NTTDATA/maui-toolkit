namespace Ndbs.MauiToolkit.Diff
{
	public class DiffDescription<T>
	{
		public DiffTypes Type { get; set; }
		public T? Local { get; set; }
		public T? Remote { get; set; }
	}
}

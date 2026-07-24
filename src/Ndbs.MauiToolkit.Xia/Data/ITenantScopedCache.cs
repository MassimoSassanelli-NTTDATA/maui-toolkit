namespace Ndbs.MauiToolkit.Xia.Data
{
    /// <summary>
    /// A cache whose entries are scoped to the active tenant. It is cleared during a
    /// tenant switch so that data of one tenant never leaks into another.
    /// </summary>
    public interface ITenantScopedCache
    {
        /// <summary>Clears all tenant-scoped cache entries.</summary>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}

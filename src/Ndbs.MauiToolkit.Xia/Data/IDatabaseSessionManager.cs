namespace Ndbs.MauiToolkit.Xia.Data
{
    /// <summary>
    /// Manages the active local SQLite database connection(s). The databases live at
    /// the tenant level (<see cref="Workspace.TenantWorkspace.MaintenanceDbPath"/> and
    /// <see cref="Workspace.TenantWorkspace.MicroAppDbPath"/>). During a tenant switch
    /// the current connection is closed first.
    /// </summary>
    /// <remarks>
    /// The concrete EF Core / SQLite wiring is part of a later development step; this
    /// contract lets the tenant switch orchestration close connections safely.
    /// </remarks>
    public interface IDatabaseSessionManager
    {
        /// <summary>Closes the current database connection, if any.</summary>
        Task CloseCurrentAsync(CancellationToken cancellationToken = default);
    }
}

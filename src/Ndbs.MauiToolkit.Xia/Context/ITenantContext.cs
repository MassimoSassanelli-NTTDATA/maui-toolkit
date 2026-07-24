using NDBS.Xia.Api.Dtos;
using Ndbs.MauiToolkit.Xia.Workspace;

namespace Ndbs.MauiToolkit.Xia.Context
{
    /// <summary>
    /// The central truth for the currently active tenant ("TenantContext").
    /// All API services, repositories, sync services and view models obtain the
    /// active tenant from this context (or from providers derived from it) rather
    /// than passing the <see cref="System.Guid"/> tenant id around manually.
    /// </summary>
    /// <remarks>
    /// The tenant must never be changed by directly assigning a property. Use the
    /// <see cref="Switching.ITenantSwitchCoordinator"/> to perform a controlled
    /// tenant switch.
    /// </remarks>
    public interface ITenantContext
    {
        /// <summary>
        /// Gets the currently active tenant, or <see langword="null"/> when no
        /// tenant has been activated yet.
        /// </summary>
        TenantDto? CurrentTenant { get; }

        /// <summary>
        /// Gets the local storage workspace of the currently active tenant, or
        /// <see langword="null"/> when no tenant has been activated yet.
        /// </summary>
        TenantWorkspace? CurrentWorkspace { get; }

        /// <summary>
        /// Gets a value indicating whether a tenant is currently active.
        /// </summary>
        bool HasTenant { get; }

        /// <summary>
        /// Sets the currently active tenant together with its workspace. This is
        /// intended to be called by the tenant switch coordinator only.
        /// </summary>
        /// <param name="tenant">The tenant to activate.</param>
        /// <param name="workspace">The workspace associated with the tenant.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetCurrentTenantAsync(TenantDto tenant, TenantWorkspace workspace, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the currently active tenant (for example on sign-out).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}

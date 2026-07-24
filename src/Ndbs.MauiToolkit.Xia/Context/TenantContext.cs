using NDBS.Xia.Api.Dtos;
using Ndbs.MauiToolkit.Xia.Workspace;

namespace Ndbs.MauiToolkit.Xia.Context
{
    /// <summary>
    /// Default in-memory implementation of <see cref="ITenantContext"/>.
    /// </summary>
    public sealed class TenantContext : ITenantContext
    {
        /// <inheritdoc />
        public TenantDto? CurrentTenant { get; private set; }

        /// <inheritdoc />
        public TenantWorkspace? CurrentWorkspace { get; private set; }

        /// <inheritdoc />
        public bool HasTenant => CurrentTenant is not null;

        /// <inheritdoc />
        public Task SetCurrentTenantAsync(TenantDto tenant, TenantWorkspace workspace, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tenant);
            ArgumentNullException.ThrowIfNull(workspace);

            CurrentTenant = tenant;
            CurrentWorkspace = workspace;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            CurrentTenant = null;
            CurrentWorkspace = null;
            return Task.CompletedTask;
        }
    }
}

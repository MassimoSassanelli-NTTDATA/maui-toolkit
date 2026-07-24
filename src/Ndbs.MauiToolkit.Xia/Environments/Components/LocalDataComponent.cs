using System.Text.Json;
using Ndbs.MauiToolkit.Environments;
using Ndbs.MauiToolkit.Xia.Data;

namespace Ndbs.MauiToolkit.Xia.Environments.Components
{
    /// <summary>
    /// A section-less environment component that discards environment-scoped local
    /// data on tear-down: it clears the tenant-scoped cache and closes the current
    /// database session so no environment-dependent buffer survives an environment
    /// switch. It never applies configuration; it only participates in reset.
    /// </summary>
    public sealed class LocalDataComponent : ISystemEnvironmentComponent
    {
        private readonly ITenantScopedCache _tenantScopedCache;
        private readonly IDatabaseSessionManager _databaseSessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDataComponent"/> class.
        /// </summary>
        /// <param name="tenantScopedCache">The environment/tenant-scoped cache.</param>
        /// <param name="databaseSessionManager">The database session manager.</param>
        public LocalDataComponent(
            ITenantScopedCache tenantScopedCache,
            IDatabaseSessionManager databaseSessionManager)
        {
            _tenantScopedCache = tenantScopedCache ?? throw new ArgumentNullException(nameof(tenantScopedCache));
            _databaseSessionManager = databaseSessionManager ?? throw new ArgumentNullException(nameof(databaseSessionManager));
        }

        /// <inheritdoc />
        public string SectionKey => string.Empty;

        /// <inheritdoc />
        public string? Category => null;

        /// <inheritdoc />
        public bool IsRequired => false;

        /// <inheritdoc />
        public int Order => 90;

        /// <inheritdoc />
        public EnvironmentComponentValidation Validate(JsonElement section) =>
            EnvironmentComponentValidation.Valid();

        /// <inheritdoc />
        public Task ApplyAsync(JsonElement section, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        /// <inheritdoc />
        public async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            await _tenantScopedCache.ClearAsync(cancellationToken).ConfigureAwait(false);
            await _databaseSessionManager.CloseCurrentAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

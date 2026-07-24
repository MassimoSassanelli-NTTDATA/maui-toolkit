using Ndbs.MauiToolkit.Context;
using Ndbs.MauiToolkit.Xia.Context;
using Ndbs.MauiToolkit.Xia.Workspace;

namespace Ndbs.MauiToolkit.Xia.Initialization
{
    /// <summary>
    /// The micro-app stage of the XIA context chain. Once a tenant is active and a
    /// micro app has been selected, it opens (creating if necessary) the micro app
    /// workspace and publishes it through <see cref="IMicroAppContext"/>.
    /// Deactivation clears the micro-app context.
    /// </summary>
    public sealed class MicroAppContextStage : ContextStageBase
    {
        private readonly IUserContext _userContext;
        private readonly ITenantContext _tenantContext;
        private readonly IMicroAppContext _microAppContext;
        private readonly ITenantWorkspaceProvider _workspaceProvider;
        private readonly MicroAppSelection _selection;
        private readonly int _order;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicroAppContextStage"/> class.
        /// </summary>
        /// <param name="userContext">The user context (prerequisite).</param>
        /// <param name="tenantContext">The tenant context (prerequisite).</param>
        /// <param name="microAppContext">The micro-app context to build up / clear.</param>
        /// <param name="workspaceProvider">Creates the micro app workspace.</param>
        /// <param name="selection">The desired micro app for the next activation.</param>
        /// <param name="order">The position of this stage in the chain, defined by the app.</param>
        public MicroAppContextStage(
            IUserContext userContext,
            ITenantContext tenantContext,
            IMicroAppContext microAppContext,
            ITenantWorkspaceProvider workspaceProvider,
            MicroAppSelection selection,
            int order)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _microAppContext = microAppContext ?? throw new ArgumentNullException(nameof(microAppContext));
            _workspaceProvider = workspaceProvider ?? throw new ArgumentNullException(nameof(workspaceProvider));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _order = order;
        }

        /// <inheritdoc />
        public override int Order => _order;

        /// <inheritdoc />
        public override string Name => "MicroApp";

        /// <inheritdoc />
        protected override async Task<ContextActivation> OnActivateAsync(CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            var tenant = _tenantContext.CurrentTenant;
            if (string.IsNullOrEmpty(userId) || tenant is null)
            {
                return ContextActivation.Deferred;
            }

            var microAppName = _selection.DesiredMicroApp ?? _microAppContext.CurrentMicroApp;
            if (string.IsNullOrWhiteSpace(microAppName))
            {
                // No micro app selected yet – a selection is required.
                return ContextActivation.Deferred;
            }

            var workspace = await _workspaceProvider
                .GetOrCreateMicroAppAsync(userId, tenant.Id, tenant.Name, microAppName, cancellationToken)
                .ConfigureAwait(false);

            await _microAppContext
                .SetCurrentMicroAppAsync(microAppName, workspace, cancellationToken)
                .ConfigureAwait(false);

            return ContextActivation.Activated;
        }

        /// <inheritdoc />
        protected override Task OnDeactivateAsync(CancellationToken cancellationToken)
            => _microAppContext.ClearAsync(cancellationToken);
    }
}

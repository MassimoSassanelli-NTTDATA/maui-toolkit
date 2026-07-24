using NDBS.Xia.Api.Dtos;
using Ndbs.MauiToolkit.Context;
using Ndbs.MauiToolkit.Xia.Context;
using Ndbs.MauiToolkit.Xia.Switching;

namespace Ndbs.MauiToolkit.Xia.Initialization
{
    /// <summary>
    /// The tenant stage of the XIA context chain. It resolves the tenant that should
    /// be active and performs a controlled switch through the
    /// <see cref="ITenantSwitchCoordinator"/> (which opens the workspace, sets the
    /// tenant context, persists the last tenant and notifies the app). Deactivation
    /// clears the tenant context.
    /// </summary>
    public sealed class TenantContextStage : ContextStageBase
    {
        private readonly IUserContext _userContext;
        private readonly ITenantContext _tenantContext;
        private readonly ITenantSwitchCoordinator _switchCoordinator;
        private readonly TenantSelection _selection;
        private readonly XiaTenantConfiguration _environmentTenant;
        private readonly int _order;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantContextStage"/> class.
        /// </summary>
        /// <param name="userContext">The user context (prerequisite).</param>
        /// <param name="tenantContext">The tenant context to build up / clear.</param>
        /// <param name="switchCoordinator">Performs the controlled tenant switch.</param>
        /// <param name="selection">The desired tenant for the next activation.</param>
        /// <param name="environmentTenant">The tenant configured by the environment.</param>
        /// <param name="order">The position of this stage in the chain, defined by the app.</param>
        public TenantContextStage(
            IUserContext userContext,
            ITenantContext tenantContext,
            ITenantSwitchCoordinator switchCoordinator,
            TenantSelection selection,
            XiaTenantConfiguration environmentTenant,
            int order)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _switchCoordinator = switchCoordinator ?? throw new ArgumentNullException(nameof(switchCoordinator));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _environmentTenant = environmentTenant ?? throw new ArgumentNullException(nameof(environmentTenant));
            _order = order;
        }

        /// <inheritdoc />
        public override int Order => _order;

        /// <inheritdoc />
        public override string Name => "Tenant";

        /// <inheritdoc />
        protected override async Task<ContextActivation> OnActivateAsync(CancellationToken cancellationToken)
        {
            // The user is the prerequisite for the tenant (workspace paths are scoped
            // by user id). The chain guarantees this, but guard defensively.
            if (!_userContext.HasUser)
            {
                return ContextActivation.Deferred;
            }

            var target = ResolveTargetTenant();
            if (target is null)
            {
                // No tenant known yet – a selection is required.
                return ContextActivation.Deferred;
            }

            var result = await _switchCoordinator
                .SwitchTenantAsync(target, cancellationToken)
                .ConfigureAwait(false);

            return result.Outcome switch
            {
                TenantSwitchOutcome.Success => ContextActivation.Activated,
                TenantSwitchOutcome.NoChange => ContextActivation.Activated,
                _ => ContextActivation.Deferred,
            };
        }

        /// <inheritdoc />
        protected override Task OnDeactivateAsync(CancellationToken cancellationToken)
            => _tenantContext.ClearAsync(cancellationToken);

        private TenantDto? ResolveTargetTenant()
        {
            if (_selection.Desired is { } desired)
            {
                return desired;
            }

            if (_environmentTenant.TenantId is { } id)
            {
                return new TenantDto
                {
                    Id = id,
                    Name = _environmentTenant.TenantName ?? id.ToString("N"),
                };
            }

            return _tenantContext.CurrentTenant;
        }
    }
}

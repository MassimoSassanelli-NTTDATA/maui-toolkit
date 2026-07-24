using NDBS.Xia.Api.Dtos;

namespace Ndbs.MauiToolkit.Xia.Initialization
{
    /// <summary>
    /// Holds the tenant that should become active on the next activation of the
    /// tenant stage. It is the hand-off point for runtime tenant switching: a tenant
    /// picker sets <see cref="Desired"/> and then triggers
    /// <c>IContextChain.ActivateFromAsync(XiaContextOrders.Tenant)</c>.
    /// </summary>
    /// <remarks>
    /// When <see cref="Desired"/> is <see langword="null"/> the tenant stage falls
    /// back to the tenant configured by the system environment
    /// (<see cref="XiaTenantConfiguration"/>) and, if that is also empty, to the
    /// currently active tenant. If none of these yield a tenant the stage defers,
    /// signalling that a tenant must be selected.
    /// </remarks>
    public sealed class TenantSelection
    {
        /// <summary>Gets or sets the tenant desired for the next activation.</summary>
        public TenantDto? Desired { get; set; }
    }
}

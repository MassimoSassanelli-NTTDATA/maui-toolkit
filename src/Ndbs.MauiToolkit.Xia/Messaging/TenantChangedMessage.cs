using NDBS.Xia.Api.Dtos;

namespace Ndbs.MauiToolkit.Xia.Messaging
{
    /// <summary>
    /// Published after a tenant switch completed successfully. View models and
    /// services react loosely coupled to this event (for example by reloading data);
    /// they must not perform the tenant switch logic themselves.
    /// </summary>
    /// <param name="NewTenant">The now active tenant.</param>
    public sealed record TenantChangedMessage(TenantDto NewTenant);
}

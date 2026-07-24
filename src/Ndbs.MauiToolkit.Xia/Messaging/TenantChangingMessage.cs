using NDBS.Xia.Api.Dtos;

namespace Ndbs.MauiToolkit.Xia.Messaging
{
    /// <summary>
    /// Published just before a tenant switch is applied. Recipients may use this to
    /// prepare for the change (the switch itself is orchestrated by the
    /// <see cref="Switching.ITenantSwitchCoordinator"/>, not by recipients).
    /// </summary>
    /// <param name="OldTenant">The previously active tenant, or <see langword="null"/>.</param>
    /// <param name="NewTenant">The tenant being switched to.</param>
    public sealed record TenantChangingMessage(TenantDto? OldTenant, TenantDto NewTenant);
}

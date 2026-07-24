namespace Ndbs.MauiToolkit.Xia.Initialization
{
    /// <summary>
    /// Holds the XIA tenant configuration loaded from the system environment.
    /// This is used to pass tenant information from the environment component
    /// (before login) to the tenant context initializer (after login).
    /// </summary>
    public sealed class XiaTenantConfiguration
    {
        /// <summary>Gets or sets the tenant ID, or <c>null</c> if no tenant is configured.</summary>
        public Guid? TenantId { get; set; }

        /// <summary>Gets or sets the tenant name (used as folder name), or <c>null</c>.</summary>
        public string? TenantName { get; set; }
    }
}

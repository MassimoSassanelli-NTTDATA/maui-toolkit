using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ndbs.MauiToolkit.Xia.Environments.Components
{
    /// <summary>
    /// The <c>Xia</c> section of a system environment. The base URL belongs to the XIA
    /// configuration itself (each API carries its own base address; there is no single
    /// shared gateway URL). The tenant is optional.
    /// </summary>
    public sealed class XiaConfig
    {
        /// <summary>Gets or sets the base URL of the XIA API for this environment.</summary>
        [JsonPropertyName("BaseUrl")]
        public string? BaseUrl { get; set; }

        /// <summary>Gets or sets the optional tenant context.</summary>
        [JsonPropertyName("Tenant")]
        public XiaTenantConfig? Tenant { get; set; }
    }

    /// <summary>The optional tenant reference inside a <see cref="XiaConfig"/>.</summary>
    public sealed class XiaTenantConfig
    {
        /// <summary>Gets or sets the tenant identifier.</summary>
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }

        /// <summary>Gets or sets the tenant name.</summary>
        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }

    /// <summary>Source-generated JSON context for <see cref="XiaConfig"/> (trimming/AOT safe).</summary>
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(XiaConfig))]
    internal sealed partial class XiaEnvironmentJsonContext : JsonSerializerContext
    {
    }
}

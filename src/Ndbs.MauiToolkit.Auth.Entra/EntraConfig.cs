using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ndbs.MauiToolkit.Auth.Entra
{
    /// <summary>
    /// The <c>AzureEntra</c> section of a system environment: the identity-provider
    /// parameters that vary per environment. The redirect URI and scopes are
    /// intentionally not part of this section, because the redirect URI is bound to
    /// the OS-registered custom URI scheme and cannot vary per environment; both stay
    /// app-level constants (consistent with the IAS section).
    /// </summary>
    public sealed class EntraConfig
    {
        /// <summary>Gets or sets the public MSAL client (application) identifier.</summary>
        [JsonPropertyName("ClientId")]
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Entra authority / issuer URL, including the tenant
        /// (for example <c>https://login.microsoftonline.com/&lt;tenant&gt;/v2.0</c>).
        /// </summary>
        [JsonPropertyName("Authority")]
        public string? Authority { get; set; }
    }

    /// <summary>Source-generated JSON context for <see cref="EntraConfig"/> (trimming/AOT safe).</summary>
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(EntraConfig))]
    internal sealed partial class EntraEnvironmentJsonContext : JsonSerializerContext
    {
    }
}

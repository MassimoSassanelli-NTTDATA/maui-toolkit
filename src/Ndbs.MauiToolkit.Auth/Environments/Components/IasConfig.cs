using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ndbs.MauiToolkit.Auth.Environments.Components
{
    /// <summary>
    /// The <c>IAS</c> section of a system environment: the identity-provider
    /// parameters that vary per environment. The redirect URI and scopes are
    /// intentionally not part of this section, because the redirect URI is bound to
    /// the OS-registered custom URI scheme and cannot vary per environment; both stay
    /// app-level constants.
    /// </summary>
    public sealed class IasConfig
    {
        /// <summary>Gets or sets the public OIDC client identifier.</summary>
        [JsonPropertyName("ClientId")]
        public string? ClientId { get; set; }

        /// <summary>Gets or sets the OIDC authority / issuer URL.</summary>
        [JsonPropertyName("Authority")]
        public string? Authority { get; set; }
    }

    /// <summary>Source-generated JSON context for <see cref="IasConfig"/> (trimming/AOT safe).</summary>
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(IasConfig))]
    internal sealed partial class IasEnvironmentJsonContext : JsonSerializerContext
    {
    }
}

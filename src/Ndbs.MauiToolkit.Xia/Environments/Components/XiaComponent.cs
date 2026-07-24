using System.Text.Json;
using NDBS.Xia.Api;
using Ndbs.MauiToolkit.Environments;
using Ndbs.MauiToolkit.Xia.Initialization;

namespace Ndbs.MauiToolkit.Xia.Environments.Components
{
    /// <summary>
    /// Environment component for the XIA backend. It re-targets the XIA API base
    /// address for the active environment and stores the tenant configuration for
    /// later activation. The tenant itself is never switched here: activation goes
    /// exclusively through the context chain (the tenant stage), which respects the
    /// dependency order defined by the app (User → Tenant → MicroApp → …) and is the
    /// single caller of the tenant switch coordinator.
    /// </summary>
    public sealed class XiaComponent : ISystemEnvironmentComponent
    {
        /// <summary>The section key handled by this component.</summary>
        public const string Section = "Xia";

        private readonly XiaApiOptionsUpdater _apiOptionsUpdater;
        private readonly XiaTenantConfiguration _tenantConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="XiaComponent"/> class.
        /// </summary>
        /// <param name="apiOptionsUpdater">Updates the XIA API base address at runtime.</param>
        /// <param name="tenantConfig">Stores tenant configuration from the environment.</param>
        public XiaComponent(
            XiaApiOptionsUpdater apiOptionsUpdater,
            XiaTenantConfiguration tenantConfig)
        {
            _apiOptionsUpdater = apiOptionsUpdater ?? throw new ArgumentNullException(nameof(apiOptionsUpdater));
            _tenantConfig = tenantConfig ?? throw new ArgumentNullException(nameof(tenantConfig));
        }

        /// <inheritdoc />
        public string SectionKey => Section;

        /// <inheritdoc />
        public string? Category => null;

        /// <inheritdoc />
        public bool IsRequired => false;

        /// <inheritdoc />
        public int Order => 20;

        /// <inheritdoc />
        public EnvironmentComponentValidation Validate(JsonElement section)
        {
            var config = Read(section);
            if (config is null)
            {
                return EnvironmentComponentValidation.Invalid("Die XIA-Konfiguration ist ungültig.");
            }

            if (!IsAbsoluteHttpUri(config.BaseUrl))
            {
                return EnvironmentComponentValidation.Invalid("Die XIA-Konfiguration enthält keine gültige BaseUrl.");
            }

            if (config.Tenant is not null && string.IsNullOrWhiteSpace(config.Tenant.Name))
            {
                return EnvironmentComponentValidation.Invalid("Der Mandant der XIA-Konfiguration benötigt einen Namen.");
            }

            return EnvironmentComponentValidation.Valid();
        }

        /// <inheritdoc />
        public Task ApplyAsync(JsonElement section, CancellationToken cancellationToken = default)
        {
            var config = Read(section)
                ?? throw new InvalidOperationException("Die XIA-Konfiguration ist ungültig.");

            var baseUri = new Uri(config.BaseUrl!, UriKind.Absolute);
            _apiOptionsUpdater.UpdatePartial(options => options.BaseUri = baseUri);

            // Store the tenant configuration for later activation via the context
            // chain. The tenant is never switched here; the tenant stage reads this
            // configuration and performs the controlled switch once the user is known.
            if (config.Tenant is { } tenant)
            {
                _tenantConfig.TenantId = tenant.Id;
                _tenantConfig.TenantName = tenant.Name;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        private static XiaConfig? Read(JsonElement section)
        {
            try
            {
                return JsonSerializer.Deserialize(section.GetRawText(), XiaEnvironmentJsonContext.Default.XiaConfig);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static bool IsAbsoluteHttpUri(string? value) =>
            Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

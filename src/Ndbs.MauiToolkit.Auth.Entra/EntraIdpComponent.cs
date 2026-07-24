using System.Text.Json;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Providers;
using Ndbs.MauiToolkit.Environments;

namespace Ndbs.MauiToolkit.Auth.Entra
{
    /// <summary>
    /// Environment component for the Azure Entra ID identity provider. It reconfigures
    /// the OIDC authority and client id at runtime through
    /// <see cref="IOidcOptionsProvider"/> (leaving the app-level redirect URI and
    /// scopes untouched), activates the Entra/MSAL provider through
    /// <see cref="IActiveIdentityProvider"/>, and signs the user out on tear-down so a
    /// session can never survive an environment switch.
    /// </summary>
    public sealed class EntraIdpComponent : ISystemEnvironmentComponent
    {
        /// <summary>The section key handled by this component.</summary>
        public const string Section = "AzureEntra";

        /// <summary>The identity-provider routing key for the Entra provider.</summary>
        public const string ProviderKey = "AzureEntra";

        /// <summary>The identity-provider category (exactly one per environment).</summary>
        public const string IdentityProviderCategory = "IdentityProvider";

        private readonly IOidcOptionsProvider _oidcOptions;
        private readonly IAuthenticationService _authenticationService;
        private readonly IActiveIdentityProvider _activeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntraIdpComponent"/> class.
        /// </summary>
        /// <param name="oidcOptions">The runtime OIDC options provider.</param>
        /// <param name="authenticationService">The authentication service (for tear-down).</param>
        /// <param name="activeProvider">The active-identity-provider state (for provider routing).</param>
        public EntraIdpComponent(
            IOidcOptionsProvider oidcOptions,
            IAuthenticationService authenticationService,
            IActiveIdentityProvider activeProvider)
        {
            _oidcOptions = oidcOptions ?? throw new ArgumentNullException(nameof(oidcOptions));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _activeProvider = activeProvider ?? throw new ArgumentNullException(nameof(activeProvider));
        }

        /// <inheritdoc />
        public string SectionKey => Section;

        /// <inheritdoc />
        public string? Category => IdentityProviderCategory;

        /// <inheritdoc />
        public bool IsRequired => true;

        /// <inheritdoc />
        public int Order => 10;

        /// <inheritdoc />
        public EnvironmentComponentValidation Validate(JsonElement section)
        {
            var config = Read(section);
            if (config is null)
            {
                return EnvironmentComponentValidation.Invalid("Die Azure-Entra-Konfiguration ist ungültig.");
            }

            if (string.IsNullOrWhiteSpace(config.ClientId))
            {
                return EnvironmentComponentValidation.Invalid("Die Azure-Entra-Konfiguration enthält keine Client-ID.");
            }

            if (!IsAbsoluteHttpUri(config.Authority))
            {
                return EnvironmentComponentValidation.Invalid("Die Azure-Entra-Konfiguration enthält keine gültige Authority-URL.");
            }

            return EnvironmentComponentValidation.Valid();
        }

        /// <inheritdoc />
        public Task ApplyAsync(JsonElement section, CancellationToken cancellationToken = default)
        {
            var config = Read(section)
                ?? throw new InvalidOperationException("Die Azure-Entra-Konfiguration ist ungültig.");

            _oidcOptions.Update(options =>
            {
                options.Authority = config.Authority!;
                options.ClientId = config.ClientId!;
            });

            // Route sign-in through the Entra (MSAL) provider for this environment.
            _activeProvider.SetActiveProvider(ProviderKey);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ResetAsync(EnvironmentSwitchOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Only sign out when the switch asks for the active session to be reset;
            // otherwise the session is kept alive across the environment switch.
            return options.ResetActiveSession
                ? _authenticationService.LogoutAsync(cancellationToken: cancellationToken)
                : Task.CompletedTask;
        }

        private static EntraConfig? Read(JsonElement section)
        {
            try
            {
                return JsonSerializer.Deserialize(section.GetRawText(), EntraEnvironmentJsonContext.Default.EntraConfig);
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

using System.Text.Json;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Providers;
using Ndbs.MauiToolkit.Environments;

namespace Ndbs.MauiToolkit.Auth.Ias
{
    /// <summary>
    /// Environment component for the IAS identity provider. It reconfigures the OIDC
    /// authority and client id at runtime through <see cref="IOidcOptionsProvider"/>
    /// (leaving the app-level redirect URI and scopes untouched) and signs the user
    /// out on tear-down so a session can never survive an environment switch.
    /// </summary>
    public sealed class IasIdpComponent : ISystemEnvironmentComponent
    {
        /// <summary>The section key handled by this component.</summary>
        public const string Section = "IAS";

        /// <summary>The identity-provider routing key for the IAS provider.</summary>
        public const string ProviderKey = "IAS";

        /// <summary>The identity-provider category (exactly one per environment).</summary>
        public const string IdentityProviderCategory = "IdentityProvider";

        private readonly IOidcOptionsProvider _oidcOptions;
        private readonly IAuthenticationService _authenticationService;
        private readonly IActiveIdentityProvider _activeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="IasIdpComponent"/> class.
        /// </summary>
        /// <param name="oidcOptions">The runtime OIDC options provider.</param>
        /// <param name="authenticationService">The authentication service (for tear-down).</param>
        /// <param name="activeProvider">The active-identity-provider state (for provider routing).</param>
        public IasIdpComponent(
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
                return EnvironmentComponentValidation.Invalid("Die IAS-Konfiguration ist ungültig.");
            }

            if (string.IsNullOrWhiteSpace(config.ClientId))
            {
                return EnvironmentComponentValidation.Invalid("Die IAS-Konfiguration enthält keine Client-ID.");
            }

            if (!IsAbsoluteHttpUri(config.Authority))
            {
                return EnvironmentComponentValidation.Invalid("Die IAS-Konfiguration enthält keine gültige Authority-URL.");
            }

            return EnvironmentComponentValidation.Valid();
        }

        /// <inheritdoc />
        public Task ApplyAsync(JsonElement section, CancellationToken cancellationToken = default)
        {
            var config = Read(section)
                ?? throw new InvalidOperationException("Die IAS-Konfiguration ist ungültig.");

            _oidcOptions.Update(options =>
            {
                options.Authority = config.Authority!;
                options.ClientId = config.ClientId!;
            });

            // Route sign-in through the IAS (Duende) provider for this environment.
            _activeProvider.SetActiveProvider(ProviderKey);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ResetAsync(CancellationToken cancellationToken = default) =>
            _authenticationService.LogoutAsync(cancellationToken: cancellationToken);

        private static IasConfig? Read(JsonElement section)
        {
            try
            {
                return JsonSerializer.Deserialize(section.GetRawText(), IasEnvironmentJsonContext.Default.IasConfig);
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

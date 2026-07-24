using Duende.IdentityModel.OidcClient;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;
using Microsoft.Extensions.Logging;
using Ndbs.MauiToolkit.Auth.Client;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tokens;

namespace Ndbs.MauiToolkit.Auth.Ias
{
    /// <summary>
    /// <see cref="IOidcClient"/> implementation backed by
    /// <c>Duende.IdentityModel.OidcClient</c>. Performs the Authorization Code Flow
    /// with PKCE and no client secret (A1, A16). The underlying client is rebuilt
    /// whenever the configuration changes at runtime (A9).
    /// </summary>
    public sealed class DuendeOidcClient : IOidcClient, IDisposable
    {
        private readonly IOidcOptionsProvider _optionsProvider;
        private readonly IBrowser _browser;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<DuendeOidcClient> _logger;
        private readonly object _gate = new();

        private OidcClient? _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuendeOidcClient"/> class.
        /// </summary>
        /// <param name="optionsProvider">The runtime OIDC configuration provider.</param>
        /// <param name="browser">The platform browser used for interactive sign-in.</param>
        /// <param name="loggerFactory">The logger factory passed to the underlying OIDC client (A13).</param>
        public DuendeOidcClient(IOidcOptionsProvider optionsProvider, IBrowser browser, ILoggerFactory loggerFactory)
        {
            _optionsProvider = optionsProvider ?? throw new ArgumentNullException(nameof(optionsProvider));
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<DuendeOidcClient>();

            _optionsProvider.OptionsChanged += OnOptionsChanged;
        }

        /// <inheritdoc />
        public async Task<OidcLoginResult> LoginAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetClient();
                var result = await client.LoginAsync(new LoginRequest(), cancellationToken).ConfigureAwait(false);

                if (result.IsError)
                {
                    _logger.LogWarning("OIDC login failed: {Error} {Description}", result.Error, result.ErrorDescription);
                    return OidcLoginResult.Failure(MapError(result.Error), DescribeError(result.Error, result.ErrorDescription));
                }

                var tokens = new OidcTokenSet
                {
                    AccessToken = result.AccessToken ?? string.Empty,
                    RefreshToken = result.RefreshToken ?? string.Empty,
                    IdentityToken = result.IdentityToken ?? string.Empty,
                    AccessTokenExpiration = result.AccessTokenExpiration,
                };

                return OidcLoginResult.Success(tokens, ExtractClaims(result.User));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OIDC login.");
                return OidcLoginResult.Failure(AuthenticationErrorCode.Unexpected, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<OidcRefreshResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return OidcRefreshResult.Failure(AuthenticationErrorCode.NoRefreshToken, "No refresh token available.");

            try
            {
                var client = GetClient();
                var result = await client.RefreshTokenAsync(refreshToken, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (result.IsError)
                {
                    _logger.LogWarning("OIDC token refresh failed: {Error} {Description}", result.Error, result.ErrorDescription);
                    return OidcRefreshResult.Failure(MapError(result.Error), DescribeError(result.Error, result.ErrorDescription));
                }

                var tokens = new OidcTokenSet
                {
                    AccessToken = result.AccessToken ?? string.Empty,
                    // The provider may not issue a new refresh token; keep the previous one.
                    RefreshToken = string.IsNullOrEmpty(result.RefreshToken) ? refreshToken : result.RefreshToken,
                    IdentityToken = result.IdentityToken ?? string.Empty,
                    AccessTokenExpiration = result.AccessTokenExpiration,
                };

                return OidcRefreshResult.Success(tokens);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OIDC token refresh.");
                return OidcRefreshResult.Failure(AuthenticationErrorCode.Unexpected, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task LogoutAsync(string? identityToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = GetClient();
                var request = new LogoutRequest { IdTokenHint = identityToken };
                await client.LogoutAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // A failed remote logout must not prevent local sign-out (A5); just log it.
                _logger.LogWarning(ex, "RP-initiated logout at the identity provider failed.");
            }
        }

        /// <inheritdoc />
        public void Dispose() => _optionsProvider.OptionsChanged -= OnOptionsChanged;

        private void OnOptionsChanged(object? sender, EventArgs e)
        {
            lock (_gate)
            {
                _client = null;
            }
        }

        private OidcClient GetClient()
        {
            lock (_gate)
            {
                return _client ??= BuildClient(_optionsProvider.Current);
            }
        }

        private OidcClient BuildClient(OidcOptions options)
        {
            var clientOptions = new OidcClientOptions
            {
                Authority = options.Authority,
                ClientId = options.ClientId,
                Scope = options.GetScopeString(),
                RedirectUri = options.RedirectUri,
                PostLogoutRedirectUri = string.IsNullOrEmpty(options.PostLogoutRedirectUri)
                    ? options.RedirectUri
                    : options.PostLogoutRedirectUri,
                Browser = _browser,
                LoggerFactory = _loggerFactory,
            };

            return new OidcClient(clientOptions);
        }

        private static IReadOnlyList<KeyValuePair<string, string>> ExtractClaims(System.Security.Claims.ClaimsPrincipal? principal)
        {
            if (principal is null)
                return Array.Empty<KeyValuePair<string, string>>();

            var claims = new List<KeyValuePair<string, string>>();
            foreach (var claim in principal.Claims)
                claims.Add(new KeyValuePair<string, string>(claim.Type, claim.Value));

            return claims;
        }

        private static AuthenticationErrorCode MapError(string? error)
        {
            if (string.IsNullOrEmpty(error))
                return AuthenticationErrorCode.Protocol;

            return error switch
            {
                "UserCancel" => AuthenticationErrorCode.Cancelled,
                "access_denied" => AuthenticationErrorCode.Cancelled,
                _ => AuthenticationErrorCode.Protocol,
            };
        }

        private static string DescribeError(string? error, string? description)
            => string.IsNullOrEmpty(description) ? (error ?? "Unknown OIDC error.") : description!;
    }
}

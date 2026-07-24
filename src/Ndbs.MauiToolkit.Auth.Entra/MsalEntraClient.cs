using Ndbs.MauiToolkit.Auth.Client;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tokens;

namespace Ndbs.MauiToolkit.Auth.Entra
{
    /// <summary>
    /// <see cref="IOidcClient"/> implementation for Azure Entra ID backed by MSAL.NET
    /// (through the <see cref="IMsalPublicClient"/> seam). Performs the interactive
    /// Authorization Code Flow with PKCE and silent renewal from the MSAL token cache,
    /// and maps the MSAL result onto the toolkit's <see cref="OidcTokenSet"/>.
    /// </summary>
    /// <remarks>
    /// MSAL manages refresh tokens inside its own cache and never exposes them, so
    /// token renewal goes through <see cref="IMsalPublicClient.AcquireTokenSilentAsync"/>
    /// rather than a raw refresh token. The produced <see cref="OidcTokenSet"/> carries
    /// <see cref="RefreshTokenSentinel"/> as its refresh token so the base
    /// authentication service treats the session as renewable and routes renewal back
    /// here.
    /// </remarks>
    public sealed class MsalEntraClient : IOidcClient
    {
        /// <summary>
        /// Sentinel stored as the refresh token so the toolkit attempts silent renewal
        /// (which is served from the MSAL cache) even though MSAL exposes no raw
        /// refresh token.
        /// </summary>
        public const string RefreshTokenSentinel = "msal-managed";

        // Reserved OIDC scopes MSAL manages itself and rejects when passed explicitly.
        private static readonly HashSet<string> ReservedScopes =
            new(StringComparer.OrdinalIgnoreCase) { "openid", "profile", "offline_access", "email" };

        private readonly IMsalPublicClient _msal;
        private readonly IOidcOptionsProvider _optionsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalEntraClient"/> class.
        /// </summary>
        /// <param name="msal">The MSAL public-client seam.</param>
        /// <param name="optionsProvider">The runtime OIDC configuration provider.</param>
        public MsalEntraClient(IMsalPublicClient msal, IOidcOptionsProvider optionsProvider)
        {
            _msal = msal ?? throw new ArgumentNullException(nameof(msal));
            _optionsProvider = optionsProvider ?? throw new ArgumentNullException(nameof(optionsProvider));
        }

        /// <inheritdoc />
        public async Task<OidcLoginResult> LoginAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _msal.AcquireTokenInteractiveAsync(ResolveScopes(), cancellationToken).ConfigureAwait(false);
                return OidcLoginResult.Success(ToTokenSet(result), result.Claims);
            }
            catch (MsalAuthenticationException ex)
            {
                return OidcLoginResult.Failure(ex.ErrorCode, ex.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return OidcLoginResult.Failure(AuthenticationErrorCode.Unexpected, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<OidcRefreshResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                // MSAL renews from its own cache; the passed refresh token is ignored.
                var result = await _msal.AcquireTokenSilentAsync(ResolveScopes(), cancellationToken).ConfigureAwait(false);
                if (result is null)
                {
                    return OidcRefreshResult.Failure(
                        AuthenticationErrorCode.NoRefreshToken,
                        "No cached account is available; interactive sign-in is required.");
                }

                return OidcRefreshResult.Success(ToTokenSet(result));
            }
            catch (MsalAuthenticationException ex)
            {
                return OidcRefreshResult.Failure(ex.ErrorCode, ex.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return OidcRefreshResult.Failure(AuthenticationErrorCode.Unexpected, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task LogoutAsync(string? identityToken, CancellationToken cancellationToken = default)
        {
            try
            {
                await _msal.SignOutAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // A failed remote sign-out must not prevent local sign-out (A5).
            }
        }

        private IReadOnlyList<string> ResolveScopes()
        {
            var resolved = new List<string>();
            foreach (var scope in _optionsProvider.Current.Scopes)
            {
                if (!string.IsNullOrWhiteSpace(scope) && !ReservedScopes.Contains(scope))
                {
                    resolved.Add(scope);
                }
            }

            return resolved;
        }

        private static OidcTokenSet ToTokenSet(MsalAuthResult result) => new()
        {
            AccessToken = result.AccessToken,
            IdentityToken = result.IdToken,
            RefreshToken = RefreshTokenSentinel,
            AccessTokenExpiration = result.ExpiresOn,
        };
    }
}

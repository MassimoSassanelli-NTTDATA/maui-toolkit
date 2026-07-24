using Microsoft.Identity.Client;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Results;
using MsalResult = Microsoft.Identity.Client.AuthenticationResult;

namespace Ndbs.MauiToolkit.Auth.Entra.Msal
{
    /// <summary>
    /// <see cref="IMsalPublicClient"/> implementation backed by
    /// <c>Microsoft.Identity.Client</c> (MSAL.NET). Builds an
    /// <see cref="IPublicClientApplication"/> from the current OIDC options and rebuilds
    /// it whenever the configuration changes at runtime (for example on an environment
    /// switch). Translates MSAL exceptions into the provider-neutral
    /// <see cref="MsalAuthenticationException"/>.
    /// </summary>
    /// <remarks>
    /// Interactive sign-in on Android and iOS requires a platform parent window; the
    /// app can supply one through the optional parent-window provider. On Windows the
    /// broker/embedded experience is used.
    /// </remarks>
    public sealed class MsalPublicClientAdapter : IMsalPublicClient, IDisposable
    {
        private readonly IOidcOptionsProvider _optionsProvider;
        private readonly Func<object?>? _parentWindowProvider;
        private readonly object _gate = new();

        private IPublicClientApplication? _app;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalPublicClientAdapter"/> class.
        /// </summary>
        /// <param name="optionsProvider">The runtime OIDC configuration provider.</param>
        /// <param name="parentWindowProvider">
        /// Optional provider of the platform parent window/activity for interactive
        /// sign-in (required on Android/iOS).
        /// </param>
        public MsalPublicClientAdapter(IOidcOptionsProvider optionsProvider, Func<object?>? parentWindowProvider = null)
        {
            _optionsProvider = optionsProvider ?? throw new ArgumentNullException(nameof(optionsProvider));
            _parentWindowProvider = parentWindowProvider;
            _optionsProvider.OptionsChanged += OnOptionsChanged;
        }

        /// <inheritdoc />
        public async Task<MsalAuthResult> AcquireTokenInteractiveAsync(IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
        {
            var app = GetApp();
            try
            {
                var request = app.AcquireTokenInteractive(scopes);

                var parent = _parentWindowProvider?.Invoke();
                if (parent is not null)
                {
                    request = request.WithParentActivityOrWindow(parent);
                }

                var result = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                return Map(result);
            }
            catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
            {
                throw new MsalAuthenticationException(AuthenticationErrorCode.Cancelled, "Die Anmeldung wurde abgebrochen.");
            }
            catch (MsalException ex)
            {
                throw new MsalAuthenticationException(AuthenticationErrorCode.Protocol, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<MsalAuthResult?> AcquireTokenSilentAsync(IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
        {
            var app = GetApp();

            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
            var account = accounts.FirstOrDefault();
            if (account is null)
            {
                return null;
            }

            try
            {
                var result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync(cancellationToken).ConfigureAwait(false);
                return Map(result);
            }
            catch (MsalUiRequiredException)
            {
                // Interactive sign-in is required; signal by returning null.
                return null;
            }
            catch (MsalException ex)
            {
                throw new MsalAuthenticationException(AuthenticationErrorCode.Protocol, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            var app = GetApp();
            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
            foreach (var account in accounts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await app.RemoveAsync(account).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose() => _optionsProvider.OptionsChanged -= OnOptionsChanged;

        private static MsalAuthResult Map(MsalResult result)
        {
            var claims = new List<KeyValuePair<string, string>>();
            if (result.ClaimsPrincipal is not null)
            {
                foreach (var claim in result.ClaimsPrincipal.Claims)
                {
                    claims.Add(new KeyValuePair<string, string>(claim.Type, claim.Value));
                }
            }

            return new MsalAuthResult
            {
                AccessToken = result.AccessToken,
                IdToken = result.IdToken ?? string.Empty,
                ExpiresOn = result.ExpiresOn,
                Claims = claims,
            };
        }

        private void OnOptionsChanged(object? sender, EventArgs e)
        {
            lock (_gate)
            {
                _app = null;
            }
        }

        private IPublicClientApplication GetApp()
        {
            lock (_gate)
            {
                return _app ??= BuildApp(_optionsProvider.Current);
            }
        }

        private static IPublicClientApplication BuildApp(OidcOptions options)
        {
            var builder = PublicClientApplicationBuilder.Create(options.ClientId);

            if (Uri.TryCreate(options.Authority, UriKind.Absolute, out var authority))
            {
                builder = builder.WithAuthority(authority);
            }

            if (!string.IsNullOrEmpty(options.RedirectUri))
            {
                builder = builder.WithRedirectUri(options.RedirectUri);
            }

            return builder.Build();
        }
    }
}

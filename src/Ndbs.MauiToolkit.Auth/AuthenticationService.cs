using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Ndbs.MauiToolkit.Auth.Client;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Connectivity;
using Ndbs.MauiToolkit.Auth.Messaging;
using Ndbs.MauiToolkit.Auth.Profile;
using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tokens;

namespace Ndbs.MauiToolkit.Auth
{
    /// <summary>
    /// Default <see cref="IAuthenticationService"/> implementation. Orchestrates the
    /// OIDC client, the token store, network awareness and state notifications.
    /// </summary>
    public sealed class AuthenticationService : IAuthenticationService, IDisposable
    {
        private readonly IOidcClient _oidcClient;
        private readonly ITokenStore _tokenStore;
        private readonly IOidcOptionsProvider _optionsProvider;
        private readonly INetworkConnectivity _connectivity;
        private readonly IMessenger _messenger;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="oidcClient">The OIDC protocol client.</param>
        /// <param name="tokenStore">The secure token store.</param>
        /// <param name="optionsProvider">The OIDC configuration provider.</param>
        /// <param name="connectivity">The network status provider (A10).</param>
        /// <param name="messenger">The messenger used to publish state changes (A15).</param>
        /// <param name="logger">The logger (A13).</param>
        /// <param name="timeProvider">The time source. Defaults to <see cref="TimeProvider.System"/>.</param>
        public AuthenticationService(
            IOidcClient oidcClient,
            ITokenStore tokenStore,
            IOidcOptionsProvider optionsProvider,
            INetworkConnectivity connectivity,
            IMessenger messenger,
            ILogger<AuthenticationService> logger,
            TimeProvider? timeProvider = null)
        {
            _oidcClient = oidcClient ?? throw new ArgumentNullException(nameof(oidcClient));
            _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
            _optionsProvider = optionsProvider ?? throw new ArgumentNullException(nameof(optionsProvider));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        /// <inheritdoc />
        public async Task<AuthenticationResult> LoginAsync(CancellationToken cancellationToken = default)
        {
            if (!_connectivity.IsConnected)
            {
                _logger.LogInformation("Login requested while offline.");
                return AuthenticationResult.Failure(AuthenticationErrorCode.Offline, "A network connection is required to sign in.");
            }

            _logger.LogInformation("Starting interactive sign-in.");
            var loginResult = await _oidcClient.LoginAsync(cancellationToken).ConfigureAwait(false);

            if (!loginResult.IsSuccess || loginResult.Tokens is null)
            {
                return AuthenticationResult.Failure(loginResult.ErrorCode, loginResult.ErrorMessage ?? "Sign-in failed.");
            }

            await _tokenStore.SaveAsync(loginResult.Tokens, cancellationToken).ConfigureAwait(false);

            var profile = UserProfileFactory.Create(loginResult.Claims, _optionsProvider.Current.ClaimMappings);
            _logger.LogInformation("Sign-in succeeded for subject {Subject}.", profile.Subject);

            Publish(AuthenticationState.SignedIn);
            return AuthenticationResult.Success(profile);
        }

        /// <inheritdoc />
        public async Task<string?> GetAccessTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            var tokens = await _tokenStore.GetAsync(cancellationToken).ConfigureAwait(false);
            if (tokens is null)
                return null;

            if (!forceRefresh && tokens.IsAccessTokenValid(_timeProvider.GetUtcNow()))
                return tokens.AccessToken;

            return await RefreshAccessTokenAsync(forceRefresh, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task LogoutAsync(bool remoteLogout = false, CancellationToken cancellationToken = default)
        {
            var tokens = await _tokenStore.GetAsync(cancellationToken).ConfigureAwait(false);

            var shouldRemoteLogout = remoteLogout || _optionsProvider.Current.EnableRpInitiatedLogout;
            if (shouldRemoteLogout && tokens is not null && _connectivity.IsConnected)
            {
                await _oidcClient.LogoutAsync(tokens.IdentityToken, cancellationToken).ConfigureAwait(false);
            }

            await _tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("User signed out.");

            Publish(AuthenticationState.SignedOut);
        }

        /// <inheritdoc />
        public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
        {
            var tokens = await _tokenStore.GetAsync(cancellationToken).ConfigureAwait(false);
            if (tokens is null)
                return false;

            return tokens.IsAccessTokenValid(_timeProvider.GetUtcNow()) || tokens.HasRefreshToken;
        }

        /// <inheritdoc />
        public async Task<UserProfile?> GetUserProfileAsync(CancellationToken cancellationToken = default)
        {
            var tokens = await _tokenStore.GetAsync(cancellationToken).ConfigureAwait(false);
            if (tokens is null || string.IsNullOrEmpty(tokens.IdentityToken))
                return null;

            var claims = JwtClaimsReader.Read(tokens.IdentityToken);
            return UserProfileFactory.Create(claims, _optionsProvider.Current.ClaimMappings);
        }

        /// <inheritdoc />
        public void Dispose() => _refreshLock.Dispose();

        private async Task<string?> RefreshAccessTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
        {
            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Re-read inside the lock: another caller may have refreshed already (A4).
                var tokens = await _tokenStore.GetAsync(cancellationToken).ConfigureAwait(false);
                if (tokens is null)
                    return null;

                if (!forceRefresh && tokens.IsAccessTokenValid(_timeProvider.GetUtcNow()))
                    return tokens.AccessToken;

                if (!tokens.HasRefreshToken)
                {
                    _logger.LogInformation("No refresh token available; session has expired.");
                    await _tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                    Publish(AuthenticationState.SessionExpired);
                    return null;
                }

                if (!_connectivity.IsConnected)
                {
                    _logger.LogInformation("Token refresh skipped while offline.");
                    return null;
                }

                _logger.LogInformation("Refreshing access token.");
                var result = await _oidcClient.RefreshTokenAsync(tokens.RefreshToken, cancellationToken).ConfigureAwait(false);

                if (!result.IsSuccess || result.Tokens is null)
                {
                    _logger.LogWarning("Token refresh failed: {Error}", result.ErrorMessage);
                    await _tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                    Publish(AuthenticationState.SessionExpired);
                    return null;
                }

                await _tokenStore.SaveAsync(result.Tokens, cancellationToken).ConfigureAwait(false);
                return result.Tokens.AccessToken;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private void Publish(AuthenticationState state)
            => _messenger.Send(new AuthenticationStateChangedMessage(state));
    }
}

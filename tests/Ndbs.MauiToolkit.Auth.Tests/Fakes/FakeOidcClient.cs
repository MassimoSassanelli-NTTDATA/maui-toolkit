using Ndbs.MauiToolkit.Auth.Client;
using Ndbs.MauiToolkit.Auth.Results;
using Ndbs.MauiToolkit.Auth.Tokens;

namespace Ndbs.MauiToolkit.Auth.Tests.Fakes
{
    /// <summary>
    /// Configurable <see cref="IOidcClient"/> that records invocations and returns
    /// queued results. Refreshes can be delayed to exercise concurrency (A4).
    /// </summary>
    internal sealed class FakeOidcClient : IOidcClient
    {
        private readonly Func<OidcLoginResult>? _loginFactory;
        private readonly Func<string, OidcRefreshResult>? _refreshFactory;

        public FakeOidcClient(
            Func<OidcLoginResult>? loginFactory = null,
            Func<string, OidcRefreshResult>? refreshFactory = null)
        {
            _loginFactory = loginFactory;
            _refreshFactory = refreshFactory;
        }

        public int LoginCount { get; private set; }

        public int RefreshCount { get; private set; }

        public int LogoutCount { get; private set; }

        public string? LastLogoutIdToken { get; private set; }

        public TimeSpan RefreshDelay { get; set; } = TimeSpan.Zero;

        public async Task<OidcLoginResult> LoginAsync(CancellationToken cancellationToken = default)
        {
            LoginCount++;
            await Task.Yield();
            return _loginFactory?.Invoke()
                ?? OidcLoginResult.Failure(AuthenticationErrorCode.Unexpected, "no login configured");
        }

        public async Task<OidcRefreshResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshCount++;
            if (RefreshDelay > TimeSpan.Zero)
                await Task.Delay(RefreshDelay, cancellationToken).ConfigureAwait(false);

            return _refreshFactory?.Invoke(refreshToken)
                ?? OidcRefreshResult.Failure(AuthenticationErrorCode.Unexpected, "no refresh configured");
        }

        public Task LogoutAsync(string? identityToken, CancellationToken cancellationToken = default)
        {
            LogoutCount++;
            LastLogoutIdToken = identityToken;
            return Task.CompletedTask;
        }
    }
}

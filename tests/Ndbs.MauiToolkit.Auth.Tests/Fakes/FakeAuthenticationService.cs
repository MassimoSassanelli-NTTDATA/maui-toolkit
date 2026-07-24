using System.Net;
using Ndbs.MauiToolkit.Auth.Profile;
using Ndbs.MauiToolkit.Auth.Results;

namespace Ndbs.MauiToolkit.Auth.Tests.Fakes
{
    /// <summary>Configurable <see cref="IAuthenticationService"/> for handler tests.</summary>
    internal sealed class FakeAuthenticationService : IAuthenticationService
    {
        private readonly Queue<string?> _tokens;

        public FakeAuthenticationService(params string?[] tokens) => _tokens = new Queue<string?>(tokens);

        public List<bool> ForceRefreshCalls { get; } = new();

        public Task<AuthenticationResult> LoginAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string?> GetAccessTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            ForceRefreshCalls.Add(forceRefresh);
            var token = _tokens.Count > 0 ? _tokens.Dequeue() : null;
            return Task.FromResult(token);
        }

        public Task LogoutAsync(bool remoteLogout = false, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<UserProfile?> GetUserProfileAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<UserProfile?>(null);
    }

    /// <summary>Inner handler that records requests and returns queued responses.</summary>
    internal sealed class RecordingInnerHandler : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statusCodes;

        public RecordingInnerHandler(params HttpStatusCode[] statusCodes)
            => _statusCodes = new Queue<HttpStatusCode>(statusCodes);

        public List<string?> AuthorizationHeaders { get; } = new();

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            AuthorizationHeaders.Add(request.Headers.Authorization?.ToString());
            var status = _statusCodes.Count > 0 ? _statusCodes.Dequeue() : HttpStatusCode.OK;
            return Task.FromResult(new HttpResponseMessage(status));
        }
    }
}

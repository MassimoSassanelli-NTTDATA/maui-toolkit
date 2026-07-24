using System.Net;
using System.Net.Http.Headers;

namespace Ndbs.MauiToolkit.Auth.Http
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> that attaches the current access token as an
    /// <c>Authorization: Bearer</c> header to outgoing requests (A14). The token is
    /// obtained through silent retrieval (A3), so expired tokens are refreshed
    /// transparently. On a <c>401 Unauthorized</c> response the handler forces a
    /// single refresh and retries the request once.
    /// </summary>
    public sealed class BearerTokenHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenHandler"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service used to obtain tokens.</param>
        public BearerTokenHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var token = await _authenticationService.GetAccessTokenAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrEmpty(token))
                return response;

            // Token may have been revoked server-side; force one refresh and retry (A14).
            var refreshed = await _authenticationService.GetAccessTokenAsync(forceRefresh: true, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(refreshed) || refreshed == token)
                return response;

            response.Dispose();

            var retry = await CloneRequestAsync(request).ConfigureAwait(false);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed);
            return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
            };

            if (request.Content is not null)
            {
                var buffer = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                var content = new ByteArrayContent(buffer);
                foreach (var header in request.Content.Headers)
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                clone.Content = content;
            }

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            foreach (var option in request.Options)
                ((IDictionary<string, object?>)clone.Options)[option.Key] = option.Value;

            return clone;
        }
    }
}

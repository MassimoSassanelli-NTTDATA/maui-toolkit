using System.Net;
using Ndbs.MauiToolkit.Auth.Http;
using Ndbs.MauiToolkit.Auth.Tests.Fakes;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Http
{
    public class BearerTokenHandlerTests
    {
        private static HttpClient CreateClient(BearerTokenHandler handler, RecordingInnerHandler inner)
        {
            handler.InnerHandler = inner;
            return new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
        }

        [Fact]
        public async Task SendAsync_AttachesBearerToken()
        {
            var auth = new FakeAuthenticationService("token-1");
            var inner = new RecordingInnerHandler(HttpStatusCode.OK);
            var client = CreateClient(new BearerTokenHandler(auth), inner);

            await client.GetAsync("/resource");

            Assert.Equal("Bearer token-1", inner.AuthorizationHeaders[0]);
            Assert.Equal(1, inner.CallCount);
        }

        [Fact]
        public async Task SendAsync_NoToken_DoesNotAttachHeader()
        {
            var auth = new FakeAuthenticationService(new string?[] { null });
            var inner = new RecordingInnerHandler(HttpStatusCode.OK);
            var client = CreateClient(new BearerTokenHandler(auth), inner);

            await client.GetAsync("/resource");

            Assert.Null(inner.AuthorizationHeaders[0]);
        }

        [Fact]
        public async Task SendAsync_On401_RefreshesOnceAndRetries()
        {
            var auth = new FakeAuthenticationService("stale-token", "fresh-token");
            var inner = new RecordingInnerHandler(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
            var client = CreateClient(new BearerTokenHandler(auth), inner);

            var response = await client.GetAsync("/resource");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, inner.CallCount);
            Assert.Equal("Bearer stale-token", inner.AuthorizationHeaders[0]);
            Assert.Equal("Bearer fresh-token", inner.AuthorizationHeaders[1]);
            Assert.Contains(true, auth.ForceRefreshCalls);
        }

        [Fact]
        public async Task SendAsync_On401_WithSameTokenAfterRefresh_DoesNotRetry()
        {
            var auth = new FakeAuthenticationService("same-token", "same-token");
            var inner = new RecordingInnerHandler(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
            var client = CreateClient(new BearerTokenHandler(auth), inner);

            var response = await client.GetAsync("/resource");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(1, inner.CallCount);
        }

        [Fact]
        public async Task SendAsync_On401_WithRetainedRequestContent_ResendsBody()
        {
            var auth = new FakeAuthenticationService("stale", "fresh");
            var inner = new RecordingInnerHandler(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
            var client = CreateClient(new BearerTokenHandler(auth), inner);

            var response = await client.PostAsync("/resource", new StringContent("payload"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, inner.CallCount);
        }
    }
}

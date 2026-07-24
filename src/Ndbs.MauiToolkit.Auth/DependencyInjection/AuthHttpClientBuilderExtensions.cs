using Microsoft.Extensions.DependencyInjection;
using Ndbs.MauiToolkit.Auth.Http;

namespace Ndbs.MauiToolkit.Auth.DependencyInjection
{
    /// <summary>
    /// Extensions for binding the <see cref="BearerTokenHandler"/> to an
    /// <see cref="HttpClient"/> registered through <see cref="IHttpClientBuilder"/> (A14).
    /// </summary>
    public static class AuthHttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="BearerTokenHandler"/> to the message-handler pipeline of
        /// the named/typed <see cref="HttpClient"/>, so the current access token is
        /// attached automatically to its requests (A14).
        /// </summary>
        /// <param name="builder">The HTTP client builder.</param>
        /// <returns>The same <see cref="IHttpClientBuilder"/> for chaining.</returns>
        public static IHttpClientBuilder AddNdbsAuthBearerToken(this IHttpClientBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddHttpMessageHandler<BearerTokenHandler>();
        }
    }
}

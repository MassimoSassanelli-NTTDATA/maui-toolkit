namespace Ndbs.MauiToolkit.Auth.Entra
{
    /// <summary>
    /// Provider-neutral projection of an MSAL authentication result. Keeps the
    /// MSAL-specific types behind <see cref="IMsalPublicClient"/> so the mapping logic
    /// in <see cref="MsalEntraClient"/> stays testable without a real MSAL client.
    /// </summary>
    public sealed class MsalAuthResult
    {
        /// <summary>Gets the access token used to authorize API calls.</summary>
        public required string AccessToken { get; init; }

        /// <summary>Gets the identity token carrying the user's identity claims.</summary>
        public string IdToken { get; init; } = string.Empty;

        /// <summary>Gets the absolute instant at which the access token expires.</summary>
        public DateTimeOffset ExpiresOn { get; init; }

        /// <summary>Gets the identity claims extracted from the result.</summary>
        public IReadOnlyList<KeyValuePair<string, string>> Claims { get; init; } =
            Array.Empty<KeyValuePair<string, string>>();
    }
}

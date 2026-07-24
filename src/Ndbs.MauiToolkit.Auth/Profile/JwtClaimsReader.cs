using System.Text.Json;

namespace Ndbs.MauiToolkit.Auth.Profile
{
    /// <summary>
    /// Reads the claims contained in the payload of a JWT (such as an identity token)
    /// without external dependencies. Parsing uses <see cref="JsonDocument"/> so it
    /// remains free of runtime reflection and safe for trimming and AOT (N1).
    /// </summary>
    public static class JwtClaimsReader
    {
        /// <summary>
        /// Extracts the claims from a JWT payload segment.
        /// </summary>
        /// <param name="jwt">The compact-serialized JWT.</param>
        /// <returns>
        /// The claims as type/value pairs. Array claim values are returned as one pair
        /// per element. Returns an empty list when the token is missing or malformed.
        /// </returns>
        public static IReadOnlyList<KeyValuePair<string, string>> Read(string? jwt)
        {
            var claims = new List<KeyValuePair<string, string>>();

            if (string.IsNullOrEmpty(jwt))
                return claims;

            var segments = jwt.Split('.');
            if (segments.Length < 2)
                return claims;

            if (!TryDecodeBase64Url(segments[1], out var payload))
                return claims;

            try
            {
                using var document = JsonDocument.Parse(payload);
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    AppendValue(claims, property.Name, property.Value);
                }
            }
            catch (JsonException)
            {
                return new List<KeyValuePair<string, string>>();
            }

            return claims;
        }

        private static void AppendValue(List<KeyValuePair<string, string>> claims, string name, JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (var item in value.EnumerateArray())
                        AppendValue(claims, name, item);
                    break;
                case JsonValueKind.String:
                    claims.Add(new KeyValuePair<string, string>(name, value.GetString() ?? string.Empty));
                    break;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    claims.Add(new KeyValuePair<string, string>(name, value.GetRawText()));
                    break;
                default:
                    break;
            }
        }

        private static bool TryDecodeBase64Url(string value, out byte[] bytes)
        {
            var output = value.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4)
            {
                case 2: output += "=="; break;
                case 3: output += "="; break;
            }

            try
            {
                bytes = Convert.FromBase64String(output);
                return true;
            }
            catch (FormatException)
            {
                bytes = Array.Empty<byte>();
                return false;
            }
        }
    }
}

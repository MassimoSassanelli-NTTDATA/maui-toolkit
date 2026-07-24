using Ndbs.MauiToolkit.Auth.Configuration;

namespace Ndbs.MauiToolkit.Auth.Profile
{
    /// <summary>
    /// Builds a <see cref="UserProfile"/> from token claims using the configurable
    /// claim mappings (A6).
    /// </summary>
    public static class UserProfileFactory
    {
        /// <summary>
        /// Creates a profile by reading the configured claim types from the supplied
        /// claim collection.
        /// </summary>
        /// <param name="claims">
        /// The claims from the identity token (claim type to value). When multiple
        /// values exist for a type, the first is used.
        /// </param>
        /// <param name="mappings">The claim-to-profile mappings.</param>
        /// <returns>The mapped <see cref="UserProfile"/>.</returns>
        public static UserProfile Create(IEnumerable<KeyValuePair<string, string>> claims, OidcClaimMappings mappings)
        {
            ArgumentNullException.ThrowIfNull(claims);
            ArgumentNullException.ThrowIfNull(mappings);

            var lookup = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var claim in claims)
            {
                if (!lookup.ContainsKey(claim.Key))
                    lookup[claim.Key] = claim.Value;
            }

            return new UserProfile
            {
                Subject = Get(lookup, mappings.SubjectClaim) ?? string.Empty,
                GivenName = Get(lookup, mappings.GivenNameClaim),
                FamilyName = Get(lookup, mappings.FamilyNameClaim),
                Email = Get(lookup, mappings.EmailClaim),
            };
        }

        private static string? Get(IReadOnlyDictionary<string, string> claims, string? claimType)
            => !string.IsNullOrEmpty(claimType) && claims.TryGetValue(claimType, out var value)
                ? value
                : null;
    }
}

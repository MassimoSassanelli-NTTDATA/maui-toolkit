namespace Ndbs.MauiToolkit.Auth.Configuration
{
    /// <summary>
    /// Configures which token claim types are mapped onto the
    /// <see cref="Ndbs.MauiToolkit.Auth.Profile.UserProfile"/> fields (A6).
    /// </summary>
    /// <remarks>
    /// Different identity providers expose user information under different claim
    /// types. These mappings let an app adapt the library to any OIDC-compliant
    /// provider without changing code.
    /// </remarks>
    public sealed class OidcClaimMappings
    {
        /// <summary>
        /// Gets or sets the claim type that carries the stable user identifier.
        /// Defaults to the standard OIDC <c>sub</c> claim.
        /// </summary>
        public string SubjectClaim { get; set; } = "sub";

        /// <summary>
        /// Gets or sets the claim type that carries the given (first) name.
        /// Defaults to the standard OIDC <c>given_name</c> claim.
        /// </summary>
        public string GivenNameClaim { get; set; } = "given_name";

        /// <summary>
        /// Gets or sets the claim type that carries the family (last) name.
        /// Defaults to the standard OIDC <c>family_name</c> claim.
        /// </summary>
        public string FamilyNameClaim { get; set; } = "family_name";

        /// <summary>
        /// Gets or sets the claim type that carries the e-mail address.
        /// Defaults to the standard OIDC <c>email</c> claim.
        /// </summary>
        public string EmailClaim { get; set; } = "email";

        /// <summary>
        /// Creates a shallow copy of the current mappings.
        /// </summary>
        /// <returns>A new <see cref="OidcClaimMappings"/> with identical values.</returns>
        public OidcClaimMappings Clone() => new()
        {
            SubjectClaim = SubjectClaim,
            GivenNameClaim = GivenNameClaim,
            FamilyNameClaim = FamilyNameClaim,
            EmailClaim = EmailClaim,
        };
    }
}

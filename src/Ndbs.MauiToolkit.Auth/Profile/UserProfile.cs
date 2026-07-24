namespace Ndbs.MauiToolkit.Auth.Profile
{
    /// <summary>
    /// Identity information about the signed-in user, derived from token claims (A6).
    /// </summary>
    public sealed class UserProfile
    {
        /// <summary>
        /// Gets or sets the stable user identifier (subject).
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's given (first) name, when available.
        /// </summary>
        public string? GivenName { get; set; }

        /// <summary>
        /// Gets or sets the user's family (last) name, when available.
        /// </summary>
        public string? FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the user's e-mail address, when available.
        /// </summary>
        public string? Email { get; set; }
    }
}

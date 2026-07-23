namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// The central truth for the currently authenticated user. The user identity is
    /// established after authentication and is used, together with the active tenant,
    /// to scope local storage and the last-tenant preference.
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// Gets the stable identifier (subject) of the current user, or
        /// <see langword="null"/> when no user is signed in.
        /// </summary>
        string? UserId { get; }

        /// <summary>
        /// Gets a value indicating whether a user identity is currently available.
        /// </summary>
        bool HasUser { get; }

        /// <summary>
        /// Sets the current user identity.
        /// </summary>
        /// <param name="userId">The stable user identifier (subject).</param>
        void SetUser(string userId);

        /// <summary>
        /// Clears the current user identity (for example on sign-out).
        /// </summary>
        void Clear();
    }
}

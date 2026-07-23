namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// Default in-memory implementation of <see cref="IUserContext"/>.
    /// </summary>
    public sealed class UserContext : IUserContext
    {
        private string? _userId;

        /// <inheritdoc />
        public string? UserId => _userId;

        /// <inheritdoc />
        public bool HasUser => !string.IsNullOrEmpty(_userId);

        /// <inheritdoc />
        public void SetUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("A user id must be provided.", nameof(userId));
            }

            _userId = userId;
        }

        /// <inheritdoc />
        public void Clear() => _userId = null;
    }
}

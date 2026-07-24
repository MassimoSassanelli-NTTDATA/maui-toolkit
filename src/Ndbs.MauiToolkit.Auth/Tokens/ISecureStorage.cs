namespace Ndbs.MauiToolkit.Auth.Tokens
{
    /// <summary>
    /// Thin abstraction over a platform secure key/value store (such as MAUI
    /// <c>SecureStorage</c>). Keeping this behind an interface keeps the token
    /// storage logic platform-neutral and unit-testable (A16).
    /// </summary>
    public interface ISecureStorage
    {
        /// <summary>
        /// Reads the value associated with the supplied key.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <returns>The stored value, or <see langword="null"/> when absent.</returns>
        Task<string?> GetAsync(string key);

        /// <summary>
        /// Writes a value for the supplied key.
        /// </summary>
        /// <param name="key">The storage key.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetAsync(string key, string value);

        /// <summary>
        /// Removes the value associated with the supplied key.
        /// </summary>
        /// <param name="key">The storage key.</param>
        void Remove(string key);
    }
}

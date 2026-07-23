namespace Ndbs.MauiToolkit.Startup
{
    /// <summary>
    /// A minimal key/value store abstraction used to persist small preferences such
    /// as the last used tenant. Abstracted over the MAUI <c>Preferences</c> API so
    /// the logic can be unit tested.
    /// </summary>
    public interface IKeyValueStore
    {
        /// <summary>Gets a stored string value, or <see langword="null"/> when absent.</summary>
        string? GetString(string key);

        /// <summary>Stores a string value.</summary>
        void SetString(string key, string value);

        /// <summary>Removes a stored value.</summary>
        void Remove(string key);

        /// <summary>Gets a stored boolean value, or <paramref name="defaultValue"/> when absent.</summary>
        bool GetBool(string key, bool defaultValue = false);

        /// <summary>Stores a boolean value.</summary>
        void SetBool(string key, bool value);
    }
}

using Microsoft.Maui.Storage;

namespace Ndbs.MauiToolkit.Startup
{
    /// <summary>
    /// Default <see cref="IKeyValueStore"/> backed by the MAUI
    /// <see cref="Preferences"/> API.
    /// </summary>
    public sealed class PreferencesKeyValueStore : IKeyValueStore
    {
        /// <inheritdoc />
        public string? GetString(string key) => Preferences.Default.Get<string?>(key, null);

        /// <inheritdoc />
        public void SetString(string key, string value) => Preferences.Default.Set(key, value);

        /// <inheritdoc />
        public void Remove(string key) => Preferences.Default.Remove(key);

        /// <inheritdoc />
        public bool GetBool(string key, bool defaultValue = false) => Preferences.Default.Get(key, defaultValue);

        /// <inheritdoc />
        public void SetBool(string key, bool value) => Preferences.Default.Set(key, value);
    }
}

using Microsoft.Maui.Storage;
using ISecureStorage = Ndbs.MauiToolkit.Auth.Tokens.ISecureStorage;

namespace Ndbs.MauiToolkit.Auth.Maui
{
    /// <summary>
    /// <see cref="ISecureStorage"/> implementation backed by the MAUI
    /// <see cref="SecureStorage"/> API, which stores values in the platform secure
    /// store (Android Keystore, iOS Keychain, Windows DPAPI) (A16).
    /// </summary>
    public sealed class MauiSecureStorage : ISecureStorage
    {
        /// <inheritdoc />
        public Task<string?> GetAsync(string key) => SecureStorage.Default.GetAsync(key);

        /// <inheritdoc />
        public Task SetAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);

        /// <inheritdoc />
        public void Remove(string key) => SecureStorage.Default.Remove(key);
    }
}

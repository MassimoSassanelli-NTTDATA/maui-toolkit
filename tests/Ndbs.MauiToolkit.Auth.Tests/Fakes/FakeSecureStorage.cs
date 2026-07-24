using Ndbs.MauiToolkit.Auth.Tokens;

namespace Ndbs.MauiToolkit.Auth.Tests.Fakes
{
    /// <summary>In-memory <see cref="ISecureStorage"/> for tests.</summary>
    internal sealed class FakeSecureStorage : ISecureStorage
    {
        private readonly Dictionary<string, string> _values = new();

        public int RemoveCount { get; private set; }

        public Task<string?> GetAsync(string key)
            => Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);

        public Task SetAsync(string key, string value)
        {
            _values[key] = value;
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            RemoveCount++;
            _values.Remove(key);
        }
    }
}

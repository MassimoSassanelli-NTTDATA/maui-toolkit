using Ndbs.MauiToolkit.Startup;
using Ndbs.MauiToolkit.Workspace;

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Default <see cref="ISystemEnvironmentStore"/> that persists the environment
    /// list as a human-editable JSON file (<c>environments.json</c>) in the
    /// application data folder. The file uses the same schema-less array format as a
    /// provisioning payload, so the very same JSON can be scanned via QR code or
    /// edited by hand (which is convenient on Windows). The per-user active selection
    /// is small runtime state and is kept in the key/value store rather than in the
    /// editable file.
    /// </summary>
    public sealed class JsonFileSystemEnvironmentStore : ISystemEnvironmentStore
    {
        /// <summary>The file name of the environment list in the app data folder.</summary>
        public const string FileName = "environments.json";

        private const string ActiveKeyPrefix = "ActiveEnvironmentName:";
        private const string DeviceScope = "__device__";

        private readonly IAppDataRootProvider _appDataRootProvider;
        private readonly IKeyValueStore _keyValueStore;
        private readonly SemaphoreSlim _gate = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFileSystemEnvironmentStore"/> class.
        /// </summary>
        /// <param name="appDataRootProvider">Provides the folder for the JSON file.</param>
        /// <param name="keyValueStore">Backs the per-user active selection.</param>
        public JsonFileSystemEnvironmentStore(
            IAppDataRootProvider appDataRootProvider,
            IKeyValueStore keyValueStore)
        {
            _appDataRootProvider = appDataRootProvider ?? throw new ArgumentNullException(nameof(appDataRootProvider));
            _keyValueStore = keyValueStore ?? throw new ArgumentNullException(nameof(keyValueStore));
        }

        /// <summary>Gets the absolute path of the environment JSON file.</summary>
        public string FilePath => Path.Combine(_appDataRootProvider.GetAppDataRoot(), FileName);

        /// <inheritdoc />
        public async Task<IReadOnlyList<SystemEnvironment>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await ReadAllAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <inheritdoc />
        public async Task<SystemEnvironment?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.FirstOrDefault(e => NameEquals(e.Name, name));
        }

        /// <inheritdoc />
        public async Task AddOrUpdateAsync(SystemEnvironment environment, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(environment);

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var all = (await ReadAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
                var index = all.FindIndex(e => NameEquals(e.Name, environment.Name));
                if (index >= 0)
                {
                    all[index] = environment;
                }
                else
                {
                    all.Add(environment);
                }

                await WriteAllAsync(all, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var all = (await ReadAllAsync(cancellationToken).ConfigureAwait(false)).ToList();
                var removed = all.RemoveAll(e => NameEquals(e.Name, name));
                if (removed > 0)
                {
                    await WriteAllAsync(all, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <inheritdoc />
        public async Task<SystemEnvironment?> GetActiveAsync(string? userId, CancellationToken cancellationToken = default)
        {
            var activeName = _keyValueStore.GetString(BuildActiveKey(userId));
            if (string.IsNullOrWhiteSpace(activeName))
            {
                return null;
            }

            var match = await FindByNameAsync(activeName, cancellationToken).ConfigureAwait(false);
            if (match is null)
            {
                // The previously active environment is gone; do not keep an invalid selection.
                _keyValueStore.Remove(BuildActiveKey(userId));
            }

            return match;
        }

        /// <inheritdoc />
        public async Task SetActiveAsync(string? userId, string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("An environment name must be provided.", nameof(name));
            }

            var match = await FindByNameAsync(name, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Unbekannte Systemumgebung \"{name}\".");

            _keyValueStore.SetString(BuildActiveKey(userId), match.Name);
        }

        /// <inheritdoc />
        public Task ClearActiveAsync(string? userId, CancellationToken cancellationToken = default)
        {
            _keyValueStore.Remove(BuildActiveKey(userId));
            return Task.CompletedTask;
        }

        private async Task<IReadOnlyList<SystemEnvironment>> ReadAllAsync(CancellationToken cancellationToken)
        {
            var path = FilePath;
            if (!File.Exists(path))
            {
                return Array.Empty<SystemEnvironment>();
            }

            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            return SystemEnvironmentJsonSerializer.Deserialize(json);
        }

        private async Task WriteAllAsync(IReadOnlyList<SystemEnvironment> environments, CancellationToken cancellationToken)
        {
            var path = FilePath;
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = SystemEnvironmentJsonSerializer.Serialize(environments);

            // Write atomically: write to a temp file, then replace the target.
            var tempPath = path + ".tmp";
            await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);
            File.Copy(tempPath, path, overwrite: true);
            File.Delete(tempPath);
        }

        private static string BuildActiveKey(string? userId) =>
            ActiveKeyPrefix + (string.IsNullOrWhiteSpace(userId) ? DeviceScope : userId);

        private static bool NameEquals(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}

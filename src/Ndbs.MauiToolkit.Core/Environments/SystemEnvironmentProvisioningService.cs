namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Default <see cref="ISystemEnvironmentProvisioningService"/>. It parses the
    /// schema-less array payload, validates every environment against the registered
    /// components and, on import, merges them into the store by name.
    /// </summary>
    public sealed class SystemEnvironmentProvisioningService : ISystemEnvironmentProvisioningService
    {
        private readonly SystemEnvironmentValidator _validator;
        private readonly ISystemEnvironmentStore _store;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEnvironmentProvisioningService"/> class.
        /// </summary>
        /// <param name="validator">Validates environments against the registered components.</param>
        /// <param name="store">The environment store.</param>
        public SystemEnvironmentProvisioningService(SystemEnvironmentValidator validator, ISystemEnvironmentStore store)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <inheritdoc />
        public EnvironmentProvisioningResult Parse(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return EnvironmentProvisioningResult.Failed("Es wurden keine Konfigurationsdaten erkannt.");
            }

            IReadOnlyList<SystemEnvironment> environments;
            try
            {
                environments = SystemEnvironmentJsonSerializer.Deserialize(payload);
            }
            catch (FormatException ex)
            {
                return EnvironmentProvisioningResult.Failed(ex.Message);
            }

            if (environments.Count == 0)
            {
                return EnvironmentProvisioningResult.Failed("Die Konfigurationsdaten enthalten keine Systemumgebung.");
            }

            // Reject duplicate names within the same payload.
            var duplicate = environments
                .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate is not null)
            {
                return EnvironmentProvisioningResult.Failed(
                    $"Die Konfigurationsdaten enthalten mehrere Umgebungen mit dem Namen \"{duplicate.Key}\".");
            }

            var validationErrors = new List<string>();
            foreach (var environment in environments)
            {
                var validation = _validator.Validate(environment);
                if (!validation.IsValid)
                {
                    validationErrors.AddRange(validation.Errors);
                }
            }

            if (validationErrors.Count > 0)
            {
                return EnvironmentProvisioningResult.Failed(
                    string.Join(System.Environment.NewLine, validationErrors));
            }

            return EnvironmentProvisioningResult.Success(environments);
        }

        /// <inheritdoc />
        public async Task<EnvironmentProvisioningResult> ImportAsync(string payload, CancellationToken cancellationToken = default)
        {
            var result = Parse(payload);
            if (!result.IsSuccess)
            {
                return result;
            }

            foreach (var environment in result.Environments)
            {
                await _store.AddOrUpdateAsync(environment, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
    }
}

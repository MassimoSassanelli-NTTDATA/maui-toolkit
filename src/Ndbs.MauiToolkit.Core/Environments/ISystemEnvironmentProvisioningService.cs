namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Parses configuration data (for example a scanned QR-code payload) into system
    /// environments, validates them against the registered components and stores them.
    /// Invalid data is rejected with a business-level error message.
    /// </summary>
    public interface ISystemEnvironmentProvisioningService
    {
        /// <summary>
        /// Parses and validates a configuration payload without storing anything.
        /// </summary>
        /// <param name="payload">The JSON payload (an array of environments).</param>
        EnvironmentProvisioningResult Parse(string payload);

        /// <summary>
        /// Parses, validates and merges the environments from the payload into the
        /// store (adding new ones and updating existing ones by name).
        /// </summary>
        /// <param name="payload">The JSON payload (an array of environments).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<EnvironmentProvisioningResult> ImportAsync(string payload, CancellationToken cancellationToken = default);
    }
}

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// The result of provisioning (parsing and validating) one or more system
    /// environments from configuration data such as a scanned QR code.
    /// </summary>
    public sealed class EnvironmentProvisioningResult
    {
        private EnvironmentProvisioningResult(bool isSuccess, IReadOnlyList<SystemEnvironment> environments, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Environments = environments;
            ErrorMessage = errorMessage;
        }

        /// <summary>Gets a value indicating whether provisioning succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>Gets the parsed, valid environments when successful.</summary>
        public IReadOnlyList<SystemEnvironment> Environments { get; }

        /// <summary>
        /// Gets the business-level error message when provisioning failed; otherwise
        /// <see langword="null"/>.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>Creates a successful result.</summary>
        /// <param name="environments">The provisioned environments.</param>
        public static EnvironmentProvisioningResult Success(IReadOnlyList<SystemEnvironment> environments) =>
            new(true, environments ?? throw new ArgumentNullException(nameof(environments)), null);

        /// <summary>Creates a failed result carrying a business-level message.</summary>
        /// <param name="errorMessage">The user-facing error message.</param>
        public static EnvironmentProvisioningResult Failed(string errorMessage) =>
            new(false, Array.Empty<SystemEnvironment>(), errorMessage);
    }
}

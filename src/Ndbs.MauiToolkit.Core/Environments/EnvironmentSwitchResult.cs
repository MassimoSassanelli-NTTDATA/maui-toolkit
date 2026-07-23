namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// The result of a controlled system-environment switch.
    /// </summary>
    public sealed class EnvironmentSwitchResult
    {
        private EnvironmentSwitchResult(bool isSuccess, bool changed, SystemEnvironment? environment, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Changed = changed;
            Environment = environment;
            ErrorMessage = errorMessage;
        }

        /// <summary>Gets a value indicating whether the switch succeeded.</summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the active environment actually changed
        /// (<see langword="false"/> when the target was already active).
        /// </summary>
        public bool Changed { get; }

        /// <summary>Gets the environment involved in the switch.</summary>
        public SystemEnvironment? Environment { get; }

        /// <summary>
        /// Gets the business-level error message when the switch failed; otherwise
        /// <see langword="null"/>.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>Creates a successful switch result.</summary>
        /// <param name="environment">The now-active environment.</param>
        public static EnvironmentSwitchResult Success(SystemEnvironment environment) =>
            new(true, true, environment, null);

        /// <summary>Creates a result indicating the target was already active.</summary>
        /// <param name="environment">The already-active environment.</param>
        public static EnvironmentSwitchResult NoChange(SystemEnvironment environment) =>
            new(true, false, environment, null);

        /// <summary>Creates a failed switch result carrying a business-level message.</summary>
        /// <param name="errorMessage">The user-facing error message.</param>
        public static EnvironmentSwitchResult Failed(string errorMessage) =>
            new(false, false, null, errorMessage);
    }
}

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Options that carry the caller's intent for a single environment switch. They
    /// are passed to every <see cref="ISystemEnvironmentComponent.ResetAsync"/> so a
    /// component can adjust how thoroughly it tears down the previous environment.
    /// The core stays agnostic of what a component does with the intent.
    /// </summary>
    public sealed class EnvironmentSwitchOptions
    {
        /// <summary>
        /// Gets a value indicating whether components should tear down session-scoped
        /// state established for the previous environment (for example an authenticated
        /// user session). Defaults to <see langword="true"/>, preserving the historical
        /// behaviour where a switch always performs a full tear-down. When
        /// <see langword="false"/>, components that support it keep the active session
        /// alive across the switch.
        /// </summary>
        public bool ResetActiveSession { get; init; } = true;

        /// <summary>
        /// Gets the default options (full tear-down), used when a caller switches
        /// without specifying any options.
        /// </summary>
        public static EnvironmentSwitchOptions Default { get; } = new();
    }
}

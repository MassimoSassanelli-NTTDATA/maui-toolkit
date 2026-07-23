namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// The result of resolving the environment startup decision, optionally carrying
    /// the active environment when the flow may continue.
    /// </summary>
    public sealed class EnvironmentStartupResult
    {
        private EnvironmentStartupResult(EnvironmentStartupDecision decision, SystemEnvironment? activeEnvironment)
        {
            Decision = decision;
            ActiveEnvironment = activeEnvironment;
        }

        /// <summary>Gets the startup decision.</summary>
        public EnvironmentStartupDecision Decision { get; }

        /// <summary>
        /// Gets the active environment when <see cref="Decision"/> is
        /// <see cref="EnvironmentStartupDecision.Continue"/>; otherwise <see langword="null"/>.
        /// </summary>
        public SystemEnvironment? ActiveEnvironment { get; }

        /// <summary>Creates a result requesting the capture flow.</summary>
        public static EnvironmentStartupResult Capture() =>
            new(EnvironmentStartupDecision.Capture, null);

        /// <summary>Creates a result requesting the selection flow.</summary>
        public static EnvironmentStartupResult ShowSelection() =>
            new(EnvironmentStartupDecision.ShowSelection, null);

        /// <summary>Creates a result allowing the login flow to continue.</summary>
        /// <param name="activeEnvironment">The active environment.</param>
        public static EnvironmentStartupResult Continue(SystemEnvironment activeEnvironment) =>
            new(EnvironmentStartupDecision.Continue, activeEnvironment ?? throw new ArgumentNullException(nameof(activeEnvironment)));
    }
}

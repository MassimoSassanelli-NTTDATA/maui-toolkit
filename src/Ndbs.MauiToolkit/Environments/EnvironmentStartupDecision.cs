namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// The decision returned by the environment startup resolver, driving the app
    /// start flow before login.
    /// </summary>
    public enum EnvironmentStartupDecision
    {
        /// <summary>No environment exists yet; the capture flow must run first.</summary>
        Capture,

        /// <summary>Environments exist but none is active; a selection must be made.</summary>
        ShowSelection,

        /// <summary>An active environment exists; the regular login flow may continue.</summary>
        Continue,
    }
}

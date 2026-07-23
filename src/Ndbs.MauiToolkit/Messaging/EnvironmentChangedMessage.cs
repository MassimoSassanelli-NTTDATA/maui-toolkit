namespace Ndbs.MauiToolkit.Messaging
{
    /// <summary>
    /// Published after the active system environment has changed and the new
    /// environment has been applied.
    /// </summary>
    /// <param name="Current">The newly active environment.</param>
    public sealed record EnvironmentChangedMessage(Environments.SystemEnvironment Current);
}

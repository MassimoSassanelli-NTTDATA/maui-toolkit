namespace Ndbs.MauiToolkit.Messaging
{
    /// <summary>
    /// Published before the active system environment changes, allowing subscribers to
    /// prepare for the switch (for example to guard unsaved work).
    /// </summary>
    /// <param name="Previous">The previously active environment, or <see langword="null"/>.</param>
    /// <param name="Target">The environment being switched to.</param>
    public sealed record EnvironmentChangingMessage(
        Environments.SystemEnvironment? Previous,
        Environments.SystemEnvironment Target);
}

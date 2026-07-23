namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Decides the app start flow based on the environment status: capture when none
    /// exists, selection when some exist but none is active, otherwise continue.
    /// </summary>
    public interface ISystemEnvironmentStartupResolver
    {
        /// <summary>Resolves the environment startup decision for the given scope.</summary>
        /// <param name="userId">The user identity, or <see langword="null"/> before login (device scope).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<EnvironmentStartupResult> ResolveAsync(string? userId, CancellationToken cancellationToken = default);
    }
}

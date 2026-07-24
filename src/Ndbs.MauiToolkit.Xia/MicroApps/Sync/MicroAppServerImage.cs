namespace Ndbs.MauiToolkit.Xia.MicroApps.Sync
{
    /// <summary>
    /// The merged server-side view of a single micro app (specification §3.1). It is
    /// built by joining the <c>$meta</c> (display name / description) and <c>$head</c>
    /// (ETag) results over the micro app name.
    /// </summary>
    /// <param name="Name">The tenant-wide unique micro app name.</param>
    /// <param name="DisplayName">The display name, if provided by <c>$meta</c>.</param>
    /// <param name="Description">The description, if provided by <c>$meta</c>.</param>
    /// <param name="ETag">The server-side version tag from <c>$head</c>.</param>
    public sealed record MicroAppServerImage(
        string Name,
        string? DisplayName,
        string? Description,
        string? ETag);
}

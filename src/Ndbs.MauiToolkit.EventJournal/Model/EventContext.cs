namespace Ndbs.MauiToolkit.EventJournal.Model
{
    /// <summary>
    /// Optional, ambient context attached to every event. All members are optional:
    /// for example <see cref="TenantId"/> is only populated in scenarios that have a
    /// tenant concept (such as the XIA environment). A host typically fills these
    /// values through an <c>IEventEnricher</c>.
    /// </summary>
    public sealed record EventContext
    {
        /// <summary>An empty context (all members unset).</summary>
        public static readonly EventContext Empty = new();

        /// <summary>
        /// The tenant the event belongs to, when a tenant concept exists. May be
        /// <see langword="null"/> when no tenant logic is present.
        /// </summary>
        public Guid? TenantId { get; init; }

        /// <summary>A non-reversible hash of the user identity (never the raw id).</summary>
        public string? UserIdHash { get; init; }

        /// <summary>The application version that produced the event.</summary>
        public string? AppVersion { get; init; }

        /// <summary>The platform the event was produced on (for example "Android").</summary>
        public string? Platform { get; init; }
    }
}

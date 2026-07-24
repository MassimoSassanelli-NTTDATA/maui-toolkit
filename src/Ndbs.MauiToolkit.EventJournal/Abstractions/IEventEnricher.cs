using Ndbs.MauiToolkit.EventJournal.Model;

namespace Ndbs.MauiToolkit.EventJournal.Abstractions
{
    /// <summary>
    /// Augments the ambient <see cref="EventContext"/> of an event before it is
    /// written. Enrichers are the extension point through which a host injects
    /// scenario-specific context (for example the current tenant, app version or
    /// platform) without the journal itself depending on those concerns.
    /// </summary>
    /// <remarks>
    /// A well-behaved enricher only fills members that are still unset and returns
    /// the (possibly augmented) context. Enrichers are applied in registration order.
    /// </remarks>
    public interface IEventEnricher
    {
        /// <summary>Returns an augmented context based on the supplied one.</summary>
        /// <param name="context">The current context.</param>
        EventContext Enrich(EventContext context);
    }
}

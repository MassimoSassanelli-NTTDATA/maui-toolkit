namespace Ndbs.MauiToolkit.EventJournal.Abstractions
{
    /// <summary>
    /// Sanitizes an event payload before it is written (size limiting and, in custom
    /// implementations, redaction/masking of sensitive fields). The default
    /// implementation enforces the configured size limit only.
    /// </summary>
    public interface IEventPayloadSanitizer
    {
        /// <summary>Returns a sanitized payload that respects the size limit.</summary>
        /// <param name="payloadJson">The raw payload, or <see langword="null"/>.</param>
        /// <param name="maxBytes">The maximum UTF-8 size, or <c>0</c> for no limit.</param>
        string? Sanitize(string? payloadJson, int maxBytes);
    }
}

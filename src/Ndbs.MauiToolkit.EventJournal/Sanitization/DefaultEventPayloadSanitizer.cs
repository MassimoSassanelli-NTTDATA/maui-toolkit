using System.Text;
using Ndbs.MauiToolkit.EventJournal.Abstractions;

namespace Ndbs.MauiToolkit.EventJournal.Sanitization
{
    /// <summary>
    /// The default <see cref="IEventPayloadSanitizer"/>. It enforces the configured
    /// UTF-8 size limit by truncating on a valid character boundary and appending a
    /// marker. It performs no redaction; hosts that need masking of sensitive fields
    /// register their own sanitizer.
    /// </summary>
    public sealed class DefaultEventPayloadSanitizer : IEventPayloadSanitizer
    {
        private const string TruncationMarker = "…(truncated)";

        /// <inheritdoc />
        public string? Sanitize(string? payloadJson, int maxBytes)
        {
            if (string.IsNullOrEmpty(payloadJson) || maxBytes <= 0)
            {
                return payloadJson;
            }

            if (Encoding.UTF8.GetByteCount(payloadJson) <= maxBytes)
            {
                return payloadJson;
            }

            var markerBytes = Encoding.UTF8.GetByteCount(TruncationMarker);
            var budget = Math.Max(0, maxBytes - markerBytes);

            // Trim characters until the payload plus marker fits within the budget.
            var text = payloadJson.AsSpan();
            var length = text.Length;
            while (length > 0 && Encoding.UTF8.GetByteCount(payloadJson.AsSpan(0, length)) > budget)
            {
                length--;
            }

            return string.Concat(payloadJson.AsSpan(0, length), TruncationMarker);
        }
    }
}

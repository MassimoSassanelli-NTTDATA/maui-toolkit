using System.Text;
using Ndbs.MauiToolkit.EventJournal.Sanitization;
using Xunit;

namespace Ndbs.MauiToolkit.EventJournal.Tests
{
    public class DefaultEventPayloadSanitizerTests
    {
        private readonly DefaultEventPayloadSanitizer _sanitizer = new();

        [Fact]
        public void Sanitize_WithinLimit_ReturnsPayloadUnchanged()
        {
            var payload = "{\"a\":1}";

            var result = _sanitizer.Sanitize(payload, maxBytes: 1024);

            Assert.Equal(payload, result);
        }

        [Fact]
        public void Sanitize_WhenNull_ReturnsNull()
        {
            Assert.Null(_sanitizer.Sanitize(null, maxBytes: 1024));
        }

        [Fact]
        public void Sanitize_WhenMaxBytesZero_ReturnsPayloadUnchanged()
        {
            var payload = new string('x', 5000);

            var result = _sanitizer.Sanitize(payload, maxBytes: 0);

            Assert.Equal(payload, result);
        }

        [Fact]
        public void Sanitize_OverLimit_TruncatesWithinBudget_AndAppendsMarker()
        {
            var payload = new string('x', 500);
            const int maxBytes = 64;

            var result = _sanitizer.Sanitize(payload, maxBytes);

            Assert.NotNull(result);
            Assert.True(Encoding.UTF8.GetByteCount(result!) <= maxBytes);
            Assert.EndsWith("…(truncated)", result);
        }
    }
}

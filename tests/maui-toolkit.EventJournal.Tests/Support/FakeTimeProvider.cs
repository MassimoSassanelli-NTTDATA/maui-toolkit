namespace Ndbs.MauiToolkit.EventJournal.Tests.Support
{
    /// <summary>
    /// A deterministic <see cref="TimeProvider"/> for tests: wall-clock time and the
    /// monotonic timestamp advance together only when <see cref="Advance"/> is called,
    /// so both event timestamps and measured durations are fully controlled.
    /// </summary>
    internal sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;
        private long _timestamp;

        public FakeTimeProvider(DateTimeOffset start)
        {
            _utcNow = start;
            _timestamp = 0;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public override long GetTimestamp() => _timestamp;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public void Advance(TimeSpan by)
        {
            _utcNow = _utcNow.Add(by);
            _timestamp += (long)(by.TotalSeconds * TimestampFrequency);
        }
    }
}

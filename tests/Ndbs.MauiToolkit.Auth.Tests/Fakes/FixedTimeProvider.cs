namespace Ndbs.MauiToolkit.Auth.Tests.Fakes
{
    /// <summary>A <see cref="TimeProvider"/> returning a fixed, controllable instant.</summary>
    internal sealed class FixedTimeProvider : TimeProvider
    {
        public FixedTimeProvider(DateTimeOffset now) => Now = now;

        public DateTimeOffset Now { get; set; }

        public override DateTimeOffset GetUtcNow() => Now;
    }
}

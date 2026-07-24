namespace Ndbs.MauiToolkit.EventJournal.Model
{
    /// <summary>
    /// The lifecycle/outcome category of a journaled event. Kept intentionally small
    /// and generic so it applies to any scenario (synchronization, tenant switch,
    /// environment switch, custom app events).
    /// </summary>
    public enum EventOutcome
    {
        /// <summary>The operation has started or is in progress (point-in-time marker).</summary>
        Started = 0,

        /// <summary>The operation completed successfully.</summary>
        Succeeded = 1,

        /// <summary>The operation completed, but with a non-fatal warning.</summary>
        Warning = 2,

        /// <summary>The operation failed.</summary>
        Failed = 3,

        /// <summary>The operation was cancelled.</summary>
        Cancelled = 4,
    }
}

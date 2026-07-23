namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// The outcome of activating a single <see cref="IContextStage"/>.
    /// </summary>
    public enum ContextActivation
    {
        /// <summary>
        /// The stage activated successfully. The chain continues activating the
        /// following (downstream) stages.
        /// </summary>
        Activated = 0,

        /// <summary>
        /// The stage could not activate yet because a prerequisite or selection is
        /// missing (for example no tenant has been chosen). This is <b>not</b> an
        /// error: the chain simply stops cascading forward and leaves the remaining
        /// downstream stages inactive until the missing input becomes available.
        /// </summary>
        Deferred = 1,
    }

    /// <summary>
    /// A single stage in the <see cref="IContextChain"/>. A stage owns exactly one
    /// runtime context (for example the tenant, the active micro app or the open
    /// form) and knows how to build it up (<see cref="ActivateAsync"/>) and tear it
    /// down (<see cref="DeactivateAsync"/>).
    /// </summary>
    /// <remarks>
    /// Stages are ordered by <see cref="Order"/>. A stage may only depend on stages
    /// with a lower order. When an upstream context changes at runtime (sign-in or
    /// sign-out, tenant switch, micro-app switch, …) the chain deactivates every
    /// stage from the change point downwards in reverse order and then reactivates
    /// them in forward order. This keeps the dependency direction intact.
    /// </remarks>
    public interface IContextStage
    {
        /// <summary>
        /// Gets the ordinal position of this stage in the chain. Lower values are
        /// closer to the root (for example the user). A stage must only depend on
        /// stages with a strictly lower order.
        /// </summary>
        int Order { get; }

        /// <summary>Gets a short, stable name used for diagnostics and logging.</summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this stage is currently active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Builds up this stage's context. Called by the chain once all upstream
        /// stages are active.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>
        /// <see cref="ContextActivation.Activated"/> when the context was established
        /// and the chain may continue with downstream stages; otherwise
        /// <see cref="ContextActivation.Deferred"/>.
        /// </returns>
        Task<ContextActivation> ActivateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Tears down this stage's context. Called by the chain in reverse order
        /// before an upstream stage is re-activated or when the chain is torn down.
        /// Must be idempotent (safe to call when already inactive).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeactivateAsync(CancellationToken cancellationToken = default);
    }
}

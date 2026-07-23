namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// The result of an <see cref="IContextChain.ActivateFromAsync"/> operation.
    /// </summary>
    public sealed class ContextChainResult
    {
        private ContextChainResult(bool completed, int? deferredAtOrder, string? deferredStage)
        {
            Completed = completed;
            DeferredAtOrder = deferredAtOrder;
            DeferredStage = deferredStage;
        }

        /// <summary>
        /// Gets a value indicating whether every stage from the start order downwards
        /// activated successfully (the chain reached its end).
        /// </summary>
        public bool Completed { get; }

        /// <summary>
        /// Gets the <see cref="IContextStage.Order"/> of the stage that deferred, or
        /// <see langword="null"/> when the chain completed. Callers can use this to
        /// drive the UI (for example: show the tenant picker when the tenant stage
        /// deferred).
        /// </summary>
        public int? DeferredAtOrder { get; }

        /// <summary>
        /// Gets the <see cref="IContextStage.Name"/> of the stage that deferred, or
        /// <see langword="null"/> when the chain completed.
        /// </summary>
        public string? DeferredStage { get; }

        /// <summary>Creates a result indicating the chain activated completely.</summary>
        public static ContextChainResult Complete() => new(true, null, null);

        /// <summary>Creates a result indicating the chain stopped at a deferred stage.</summary>
        /// <param name="order">The order of the deferred stage.</param>
        /// <param name="stageName">The name of the deferred stage.</param>
        public static ContextChainResult Deferred(int order, string stageName)
            => new(false, order, stageName);
    }

    /// <summary>
    /// Orchestrates an ordered set of <see cref="IContextStage"/> instances that form
    /// a dependency chain (for example User → Tenant → MicroApp → Form). The chain is
    /// deliberately generic and free of any app- or backend-specific concepts: the
    /// concrete stages and their order are contributed by higher layers.
    /// </summary>
    /// <remarks>
    /// The chain is the single entry point for reacting to runtime context changes:
    /// <list type="bullet">
    /// <item><description>Sign-in / initial start: <c>ActivateFromAsync(firstOrder)</c>.</description></item>
    /// <item><description>Sign-out: <c>DeactivateFromAsync(firstOrder)</c>.</description></item>
    /// <item><description>Switch at a level (tenant, micro app, …): set the new
    /// selection, then <c>ActivateFromAsync(levelOrder)</c>. Every dependent stage
    /// below the level is torn down and rebuilt automatically.</description></item>
    /// </list>
    /// </remarks>
    public interface IContextChain
    {
        /// <summary>Gets the registered stages ordered by <see cref="IContextStage.Order"/>.</summary>
        IReadOnlyList<IContextStage> Stages { get; }

        /// <summary>
        /// Adds a stage to the chain. Stages are kept sorted by
        /// <see cref="IContextStage.Order"/>. Typically stages are registered through
        /// dependency injection and added automatically; this method exists for tests
        /// and advanced composition.
        /// </summary>
        /// <param name="stage">The stage to add.</param>
        void AddStage(IContextStage stage);

        /// <summary>
        /// Rebuilds the chain from <paramref name="fromOrder"/> downwards: first every
        /// stage with an order greater than or equal to <paramref name="fromOrder"/>
        /// is deactivated in reverse order, then those stages are activated again in
        /// forward order until one of them defers.
        /// </summary>
        /// <param name="fromOrder">The order from which to (re)activate.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result describing whether the chain completed or where it deferred.</returns>
        Task<ContextChainResult> ActivateFromAsync(int fromOrder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates every stage with an order greater than or equal to
        /// <paramref name="fromOrder"/> in reverse order.
        /// </summary>
        /// <param name="fromOrder">The order from which to deactivate.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeactivateFromAsync(int fromOrder, CancellationToken cancellationToken = default);
    }
}

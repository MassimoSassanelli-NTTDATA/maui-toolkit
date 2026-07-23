namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// Convenience base class for <see cref="IContextStage"/> implementations. It
    /// tracks the <see cref="IsActive"/> flag and guarantees that deactivation only
    /// runs when the stage is active, so derived classes can focus on the actual
    /// build-up and tear-down logic.
    /// </summary>
    public abstract class ContextStageBase : IContextStage
    {
        /// <inheritdoc />
        public abstract int Order { get; }

        /// <inheritdoc />
        public virtual string Name => GetType().Name;

        /// <inheritdoc />
        public bool IsActive { get; private set; }

        /// <inheritdoc />
        public async Task<ContextActivation> ActivateAsync(CancellationToken cancellationToken = default)
        {
            var result = await OnActivateAsync(cancellationToken).ConfigureAwait(false);
            IsActive = result == ContextActivation.Activated;
            return result;
        }

        /// <inheritdoc />
        public async Task DeactivateAsync(CancellationToken cancellationToken = default)
        {
            if (!IsActive)
            {
                return;
            }

            await OnDeactivateAsync(cancellationToken).ConfigureAwait(false);
            IsActive = false;
        }

        /// <summary>
        /// Builds up the stage's context. See <see cref="IContextStage.ActivateAsync"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The activation outcome.</returns>
        protected abstract Task<ContextActivation> OnActivateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Tears down the stage's context. Only invoked when the stage is active.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnDeactivateAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

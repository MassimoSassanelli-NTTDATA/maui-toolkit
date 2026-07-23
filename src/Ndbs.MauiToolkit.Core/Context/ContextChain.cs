namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// Default <see cref="IContextChain"/> implementation. Activation and
    /// deactivation are serialized with a <see cref="SemaphoreSlim"/> so that
    /// overlapping runtime changes (for example a tenant switch triggered while a
    /// sign-in is still in progress) cannot corrupt the stage order.
    /// </summary>
    public sealed class ContextChain : IContextChain
    {
        private readonly List<IContextStage> _stages = new();
        private readonly SemaphoreSlim _gate = new(1, 1);

        /// <summary>Initializes a new, empty chain.</summary>
        public ContextChain()
        {
        }

        /// <summary>
        /// Initializes a chain from the stages registered in dependency injection.
        /// </summary>
        /// <param name="stages">The stages to add.</param>
        public ContextChain(IEnumerable<IContextStage> stages)
        {
            ArgumentNullException.ThrowIfNull(stages);
            foreach (var stage in stages)
            {
                AddStageCore(stage);
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<IContextStage> Stages => _stages;

        /// <inheritdoc />
        public void AddStage(IContextStage stage)
        {
            ArgumentNullException.ThrowIfNull(stage);
            AddStageCore(stage);
        }

        /// <inheritdoc />
        public async Task<ContextChainResult> ActivateFromAsync(int fromOrder, CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Tear down everything at or below the change point first so that no
                // downstream stage keeps a stale reference to the context that is about
                // to be rebuilt.
                await DeactivateFromCoreAsync(fromOrder, cancellationToken).ConfigureAwait(false);

                foreach (var stage in _stages)
                {
                    if (stage.Order < fromOrder)
                    {
                        continue;
                    }

                    var activation = await stage.ActivateAsync(cancellationToken).ConfigureAwait(false);
                    if (activation != ContextActivation.Activated)
                    {
                        return ContextChainResult.Deferred(stage.Order, stage.Name);
                    }
                }

                return ContextChainResult.Complete();
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <inheritdoc />
        public async Task DeactivateFromAsync(int fromOrder, CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await DeactivateFromCoreAsync(fromOrder, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task DeactivateFromCoreAsync(int fromOrder, CancellationToken cancellationToken)
        {
            for (var i = _stages.Count - 1; i >= 0; i--)
            {
                if (_stages[i].Order >= fromOrder)
                {
                    await _stages[i].DeactivateAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void AddStageCore(IContextStage stage)
        {
            _stages.Add(stage);
            _stages.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        }
    }
}

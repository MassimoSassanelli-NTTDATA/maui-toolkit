using CommunityToolkit.Mvvm.Messaging;
using Ndbs.MauiToolkit.Context;
using Ndbs.MauiToolkit.Messaging;

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Default <see cref="ISystemEnvironmentSwitchCoordinator"/>. It is fully generic:
    /// it never knows the concrete environment sections and simply drives the
    /// registered <see cref="ISystemEnvironmentComponent"/> set through a deterministic
    /// reset → apply sequence.
    /// </summary>
    public sealed class SystemEnvironmentSwitchCoordinator : ISystemEnvironmentSwitchCoordinator
    {
        private readonly IReadOnlyList<ISystemEnvironmentComponent> _componentsInApplyOrder;
        private readonly SystemEnvironmentValidator _validator;
        private readonly ISystemEnvironmentStore _store;
        private readonly IUserContext _userContext;
        private readonly IMessenger _messenger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEnvironmentSwitchCoordinator"/> class.
        /// </summary>
        public SystemEnvironmentSwitchCoordinator(
            IEnumerable<ISystemEnvironmentComponent> components,
            SystemEnvironmentValidator validator,
            ISystemEnvironmentStore store,
            IUserContext userContext,
            IMessenger messenger)
        {
            ArgumentNullException.ThrowIfNull(components);
            _componentsInApplyOrder = components.OrderBy(c => c.Order).ToList();
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        }

        /// <inheritdoc />
        public async Task<EnvironmentSwitchResult> SwitchAsync(SystemEnvironment target, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(target);

            var validation = _validator.Validate(target);
            if (!validation.IsValid)
            {
                return EnvironmentSwitchResult.Failed(validation.FirstError!);
            }

            var previous = await _store.GetActiveAsync(_userContext.UserId, cancellationToken).ConfigureAwait(false);
            _messenger.Send(new EnvironmentChangingMessage(previous, target));

            // Tear down the current environment first (reverse order), so a stale
            // session or environment-scoped cache can never survive the switch.
            foreach (var component in _componentsInApplyOrder.Reverse())
            {
                await component.ResetAsync(cancellationToken).ConfigureAwait(false);
            }

            await ApplyComponentsAsync(target, cancellationToken).ConfigureAwait(false);

            await _store.SetActiveAsync(_userContext.UserId, target.Name, cancellationToken).ConfigureAwait(false);

            _messenger.Send(new EnvironmentChangedMessage(target));

            return previous is not null && NameEquals(previous.Name, target.Name)
                ? EnvironmentSwitchResult.NoChange(target)
                : EnvironmentSwitchResult.Success(target);
        }

        /// <inheritdoc />
        public Task ApplyActiveAsync(SystemEnvironment environment, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(environment);
            return ApplyComponentsAsync(environment, cancellationToken);
        }

        private async Task ApplyComponentsAsync(SystemEnvironment environment, CancellationToken cancellationToken)
        {
            foreach (var component in _componentsInApplyOrder)
            {
                if (environment.TryGetSection(component.SectionKey, out var section))
                {
                    await component.ApplyAsync(section, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static bool NameEquals(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}

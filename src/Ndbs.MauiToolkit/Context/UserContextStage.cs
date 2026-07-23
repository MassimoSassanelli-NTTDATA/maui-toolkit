namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// The root stage of a context chain. It gates the whole chain on an
    /// authenticated user: activation succeeds only once <see cref="IUserContext"/>
    /// has a user, and deactivation clears the user context (sign-out).
    /// </summary>
    /// <remarks>
    /// This stage is application-agnostic – it only depends on <see cref="IUserContext"/>.
    /// Its position in the chain (<see cref="Order"/>) is a composition concern and is
    /// therefore supplied by the composing application, not hard-coded here. Typical
    /// usage after wiring:
    /// <list type="bullet">
    /// <item><description>After sign-in: <c>userContext.SetUser(id)</c> then
    /// <c>chain.ActivateFromAsync(userOrder)</c>.</description></item>
    /// <item><description>Sign-out: <c>chain.DeactivateFromAsync(userOrder)</c> which
    /// tears down every downstream context and clears the user.</description></item>
    /// </list>
    /// </remarks>
    public sealed class UserContextStage : ContextStageBase
    {
        private readonly IUserContext _userContext;
        private readonly int _order;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserContextStage"/> class.
        /// </summary>
        /// <param name="userContext">The user context.</param>
        /// <param name="order">The position of this stage in the chain, defined by the app.</param>
        public UserContextStage(IUserContext userContext, int order)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _order = order;
        }

        /// <inheritdoc />
        public override int Order => _order;

        /// <inheritdoc />
        public override string Name => "User";

        /// <inheritdoc />
        protected override Task<ContextActivation> OnActivateAsync(CancellationToken cancellationToken)
            => Task.FromResult(_userContext.HasUser ? ContextActivation.Activated : ContextActivation.Deferred);

        /// <inheritdoc />
        protected override Task OnDeactivateAsync(CancellationToken cancellationToken)
        {
            _userContext.Clear();
            return Task.CompletedTask;
        }
    }
}

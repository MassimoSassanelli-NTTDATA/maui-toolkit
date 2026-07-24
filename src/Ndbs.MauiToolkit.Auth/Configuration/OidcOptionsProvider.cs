namespace Ndbs.MauiToolkit.Auth.Configuration
{
    /// <summary>
    /// Thread-safe default implementation of <see cref="IOidcOptionsProvider"/>.
    /// Validates options on construction and on every change (A17) and notifies
    /// subscribers when the configuration changes at runtime (A9).
    /// </summary>
    public sealed class OidcOptionsProvider : IOidcOptionsProvider
    {
        private readonly object _gate = new();
        private OidcOptions _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="OidcOptionsProvider"/> class.
        /// </summary>
        /// <param name="options">The initial configuration. Validated immediately.</param>
        public OidcOptionsProvider(OidcOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var snapshot = options.Clone();
            OidcOptionsValidator.Validate(snapshot);
            _current = snapshot;
        }

        /// <inheritdoc />
        public event EventHandler? OptionsChanged;

        /// <inheritdoc />
        public OidcOptions Current
        {
            get
            {
                lock (_gate)
                {
                    return _current.Clone();
                }
            }
        }

        /// <inheritdoc />
        public void Replace(OidcOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var snapshot = options.Clone();
            OidcOptionsValidator.Validate(snapshot);

            lock (_gate)
            {
                _current = snapshot;
            }

            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void Update(Action<OidcOptions> update)
        {
            ArgumentNullException.ThrowIfNull(update);

            OidcOptions working;
            lock (_gate)
            {
                working = _current.Clone();
            }

            update(working);
            OidcOptionsValidator.Validate(working);

            lock (_gate)
            {
                _current = working;
            }

            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

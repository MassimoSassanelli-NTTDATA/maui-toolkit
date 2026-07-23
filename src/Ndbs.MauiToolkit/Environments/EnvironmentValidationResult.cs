namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// The aggregate result of validating a whole <see cref="SystemEnvironment"/>
    /// against the registered components.
    /// </summary>
    public sealed class EnvironmentValidationResult
    {
        private EnvironmentValidationResult(bool isValid, IReadOnlyList<string> errors)
        {
            IsValid = isValid;
            Errors = errors;
        }

        /// <summary>Gets a value indicating whether the environment is valid.</summary>
        public bool IsValid { get; }

        /// <summary>Gets the collected business-level error messages.</summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>Gets the first error message, or <see langword="null"/> when valid.</summary>
        public string? FirstError => Errors.Count > 0 ? Errors[0] : null;

        /// <summary>Creates a valid result.</summary>
        public static EnvironmentValidationResult Valid() => new(true, Array.Empty<string>());

        /// <summary>Creates a failed result from the collected messages.</summary>
        /// <param name="errors">The error messages.</param>
        public static EnvironmentValidationResult Invalid(IReadOnlyList<string> errors) =>
            new(false, errors ?? throw new ArgumentNullException(nameof(errors)));
    }
}

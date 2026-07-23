namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// The result of validating a single environment section through an
    /// <see cref="ISystemEnvironmentComponent"/>.
    /// </summary>
    public sealed class EnvironmentComponentValidation
    {
        private EnvironmentComponentValidation(bool isValid, string? errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        /// <summary>Gets a value indicating whether the section is valid.</summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the user-facing, business-level error message when the section is
        /// invalid; otherwise <see langword="null"/>.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>Creates a successful validation result.</summary>
        public static EnvironmentComponentValidation Valid() => new(true, null);

        /// <summary>Creates a failed validation result carrying a business-level message.</summary>
        /// <param name="errorMessage">The user-facing error message.</param>
        public static EnvironmentComponentValidation Invalid(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException("A validation error message must be provided.", nameof(errorMessage));
            }

            return new EnvironmentComponentValidation(false, errorMessage);
        }
    }
}

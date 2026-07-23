namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// Validates a whole <see cref="SystemEnvironment"/> against the registered
    /// <see cref="ISystemEnvironmentComponent"/> set:
    /// <list type="bullet">
    /// <item>every present section must be valid for its component,</item>
    /// <item>every <see cref="ISystemEnvironmentComponent.IsRequired"/> section must be present,</item>
    /// <item>every section must be understood by a registered component,</item>
    /// <item>for each component <see cref="ISystemEnvironmentComponent.Category"/>, at most one
    /// section may be present, and required categories must have exactly one.</item>
    /// </list>
    /// </summary>
    public sealed class SystemEnvironmentValidator
    {
        private readonly IReadOnlyList<ISystemEnvironmentComponent> _components;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEnvironmentValidator"/> class.
        /// </summary>
        /// <param name="components">The registered environment components.</param>
        public SystemEnvironmentValidator(IEnumerable<ISystemEnvironmentComponent> components)
        {
            ArgumentNullException.ThrowIfNull(components);
            _components = components.ToList();
        }

        /// <summary>Validates the given environment.</summary>
        /// <param name="environment">The environment to validate.</param>
        /// <returns>The aggregate validation result.</returns>
        public EnvironmentValidationResult Validate(SystemEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(environment);

            var errors = new List<string>();
            var knownSectionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1) Per-component validation of present sections + required-section checks.
            foreach (var component in _components)
            {
                if (string.IsNullOrEmpty(component.SectionKey))
                {
                    continue;
                }

                knownSectionKeys.Add(component.SectionKey);

                var present = environment.TryGetSection(component.SectionKey, out var section);
                if (!present)
                {
                    if (component.IsRequired && !BelongsToCategory(component))
                    {
                        errors.Add($"Die Umgebung \"{environment.Name}\" enthält keine Konfiguration für \"{component.SectionKey}\".");
                    }

                    continue;
                }

                var validation = component.Validate(section);
                if (!validation.IsValid && validation.ErrorMessage is not null)
                {
                    errors.Add(validation.ErrorMessage);
                }
            }

            // 2) Unknown sections (no component registered) are rejected explicitly.
            foreach (var sectionKey in environment.Sections.Keys)
            {
                if (!knownSectionKeys.Contains(sectionKey))
                {
                    errors.Add($"Die Umgebungskomponente \"{sectionKey}\" wird von dieser App nicht unterstützt.");
                }
            }

            // 3) Category cardinality (for example exactly one identity provider).
            foreach (var group in _components
                .Where(c => !string.IsNullOrEmpty(c.Category))
                .GroupBy(c => c.Category!, StringComparer.OrdinalIgnoreCase))
            {
                var presentCount = group.Count(c => environment.HasSection(c.SectionKey));
                var categoryRequired = group.Any(c => c.IsRequired);

                if (presentCount > 1)
                {
                    errors.Add($"Die Umgebung \"{environment.Name}\" darf nur eine Komponente der Kategorie \"{group.Key}\" enthalten.");
                }
                else if (presentCount == 0 && categoryRequired)
                {
                    errors.Add($"Die Umgebung \"{environment.Name}\" benötigt genau eine Komponente der Kategorie \"{group.Key}\".");
                }
            }

            return errors.Count == 0
                ? EnvironmentValidationResult.Valid()
                : EnvironmentValidationResult.Invalid(errors);
        }

        private bool BelongsToCategory(ISystemEnvironmentComponent component) =>
            !string.IsNullOrEmpty(component.Category);
    }
}

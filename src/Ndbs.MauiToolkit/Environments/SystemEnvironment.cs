using System.Text.Json;

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// A schema-less description of a system environment. An environment carries only
    /// a unique <see cref="Name"/> plus an open set of named configuration
    /// <see cref="Sections"/> (for example <c>IAS</c>, <c>Xia</c>, additional APIs).
    /// The toolkit deliberately does not know the concrete section types; each
    /// <see cref="ISystemEnvironmentComponent"/> interprets its own section. This keeps
    /// the environment model open for future variations (other identity providers,
    /// with or without XIA, additional APIs) without changing the core.
    /// </summary>
    public sealed class SystemEnvironment
    {
        /// <summary>The name used when no section is present.</summary>
        public const string NameProperty = "Name";

        private readonly IReadOnlyDictionary<string, JsonElement> _sections;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEnvironment"/> class.
        /// </summary>
        /// <param name="name">The unique, human-readable environment name (identity).</param>
        /// <param name="sections">
        /// The configuration sections keyed by their section key. Each
        /// <see cref="JsonElement"/> must be detached (cloned) from its originating
        /// document so it stays valid for the lifetime of this instance.
        /// </param>
        public SystemEnvironment(string name, IReadOnlyDictionary<string, JsonElement> sections)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("An environment name must be provided.", nameof(name));
            }

            ArgumentNullException.ThrowIfNull(sections);

            Name = name.Trim();
            _sections = sections;
        }

        /// <summary>Gets the unique, human-readable environment name.</summary>
        public string Name { get; }

        /// <summary>Gets the configuration sections keyed by their section key.</summary>
        public IReadOnlyDictionary<string, JsonElement> Sections => _sections;

        /// <summary>
        /// Tries to get the configuration section for the given <paramref name="sectionKey"/>.
        /// </summary>
        /// <param name="sectionKey">The section key (for example <c>IAS</c>).</param>
        /// <param name="section">The section when present.</param>
        /// <returns><see langword="true"/> when the section exists.</returns>
        public bool TryGetSection(string sectionKey, out JsonElement section)
        {
            if (!string.IsNullOrEmpty(sectionKey) && _sections.TryGetValue(sectionKey, out var value))
            {
                section = value;
                return true;
            }

            section = default;
            return false;
        }

        /// <summary>
        /// Indicates whether the environment contains a section for
        /// <paramref name="sectionKey"/>.
        /// </summary>
        public bool HasSection(string sectionKey) =>
            !string.IsNullOrEmpty(sectionKey) && _sections.ContainsKey(sectionKey);
    }
}

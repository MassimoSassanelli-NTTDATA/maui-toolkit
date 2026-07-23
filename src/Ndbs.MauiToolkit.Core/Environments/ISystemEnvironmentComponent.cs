using System.Text.Json;

namespace Ndbs.MauiToolkit.Environments
{
    /// <summary>
    /// A pluggable building block that interprets one aspect (section) of a
    /// <see cref="SystemEnvironment"/>. Apps register exactly the components that
    /// describe how their environments are composed (for example an identity-provider
    /// component and an API component). The generic environment infrastructure (store,
    /// startup resolver, switch coordinator, provisioning) never knows the concrete
    /// section types; it delegates validation and activation to the components.
    /// </summary>
    public interface ISystemEnvironmentComponent
    {
        /// <summary>
        /// Gets the section key this component reads from a
        /// <see cref="SystemEnvironment"/> (for example <c>IAS</c> or <c>Xia</c>).
        /// A component that participates only in tear-down may return an empty string;
        /// it is then never applied but always reset.
        /// </summary>
        string SectionKey { get; }

        /// <summary>
        /// Gets an optional category used to express mutual-exclusion / cardinality
        /// rules across a group of components (for example <c>IdentityProvider</c>, so
        /// that exactly one of IAS or Entra ID may be present). <see langword="null"/>
        /// when the component does not belong to a group.
        /// </summary>
        string? Category { get; }

        /// <summary>
        /// Gets a value indicating whether every environment must contain this
        /// component's section. Category cardinality is evaluated separately.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// Gets the ordering used when applying components. Reset runs in reverse order.
        /// Lower values are applied first.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Validates the component's section and returns a business-level result.
        /// </summary>
        /// <param name="section">The section value for <see cref="SectionKey"/>.</param>
        EnvironmentComponentValidation Validate(JsonElement section);

        /// <summary>
        /// Applies the component's section, reconfiguring the corresponding runtime
        /// state (for example the identity provider or an API base address). Only
        /// called when the target environment contains <see cref="SectionKey"/>.
        /// </summary>
        /// <param name="section">The section value for <see cref="SectionKey"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task ApplyAsync(JsonElement section, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tears down any state established for the previously active environment (for
        /// example signing out or clearing environment-scoped caches). Always called
        /// during a controlled switch, regardless of whether the target environment
        /// contains this component's section.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task ResetAsync(CancellationToken cancellationToken = default);
    }
}

using System.Text.Json;
using Ndbs.MauiToolkit.Auth;
using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Entra;
using Ndbs.MauiToolkit.Auth.Providers;
using Ndbs.MauiToolkit.Environments;
using NSubstitute;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Entra.Tests
{
    /// <summary>
    /// Proves the "exactly one identity provider per environment" rule for the
    /// combination of the IAS and the Azure Entra providers, using the real
    /// <see cref="EntraIdpComponent"/> and a lightweight IAS-category stand-in.
    /// </summary>
    public class EntraIdentityProviderCardinalityTests
    {
        private const string EntraSectionJson = "\"AzureEntra\": {\"ClientId\":\"abc\",\"Authority\":\"https://login.microsoftonline.com/tenant/v2.0\"}";
        private const string IasSectionJson = "\"IAS\": {\"ClientId\":\"abc\",\"Authority\":\"https://ias.example.com\"}";

        private static EntraIdpComponent Entra() =>
            new(Substitute.For<IOidcOptionsProvider>(), Substitute.For<IAuthenticationService>(), new ActiveIdentityProvider());

        private static SystemEnvironmentValidator Validator() =>
            new(new ISystemEnvironmentComponent[] { Entra(), new FakeIasComponent() });

        private static SystemEnvironment Env(params string[] sectionJsonEntries)
        {
            var json = "{ \"Name\": \"E1\"" + (sectionJsonEntries.Length == 0 ? "" : ", " + string.Join(", ", sectionJsonEntries)) + " }";
            using var doc = JsonDocument.Parse(json);
            var sections = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                if (property.NameEquals(SystemEnvironment.NameProperty))
                {
                    continue;
                }

                sections[property.Name] = property.Value.Clone();
            }

            return new SystemEnvironment("E1", sections);
        }

        [Fact]
        public void OnlyEntra_IsValid()
        {
            var result = Validator().Validate(Env(EntraSectionJson));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void OnlyIas_IsValid()
        {
            var result = Validator().Validate(Env(IasSectionJson));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void BothIasAndEntra_IsInvalid()
        {
            var result = Validator().Validate(Env(IasSectionJson, EntraSectionJson));

            Assert.False(result.IsValid);
        }

        [Fact]
        public void NoIdentityProvider_IsInvalid()
        {
            var result = Validator().Validate(Env());

            Assert.False(result.IsValid);
        }

        private sealed class FakeIasComponent : ISystemEnvironmentComponent
        {
            public string SectionKey => "IAS";
            public string? Category => EntraIdpComponent.IdentityProviderCategory;
            public bool IsRequired => true;
            public int Order => 10;
            public EnvironmentComponentValidation Validate(JsonElement section) => EnvironmentComponentValidation.Valid();
            public Task ApplyAsync(JsonElement section, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task ResetAsync(EnvironmentSwitchOptions options, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}

using Ndbs.MauiToolkit.Environments;
using Xunit;

namespace Ndbs.MauiToolkit.Tests.Environments;

public sealed class SystemEnvironmentValidatorTests
{
    private static SystemEnvironment Env(string sectionsJson) =>
        EnvironmentTestSupport.ParseSingle($$"""[ { "Name": "E1", {{sectionsJson}} } ]""");

    [Fact]
    public void Validate_MissingRequiredSection_Fails()
    {
        var validator = new SystemEnvironmentValidator(new[]
        {
            new FakeComponent("Api", new List<string>(), required: true),
        });

        var result = validator.Validate(Env("\"Other\": {}"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_UnknownSection_Fails()
    {
        var validator = new SystemEnvironmentValidator(new[]
        {
            new FakeComponent("Api", new List<string>()),
        });

        var result = validator.Validate(Env("\"Unknown\": {}"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Unknown"));
    }

    [Fact]
    public void Validate_TwoComponentsSameCategory_Fails()
    {
        var log = new List<string>();
        var validator = new SystemEnvironmentValidator(new[]
        {
            new FakeComponent("IAS", log, category: "IdentityProvider", required: true),
            new FakeComponent("Entra", log, category: "IdentityProvider", required: true),
        });

        var result = validator.Validate(Env("\"IAS\": {}, \"Entra\": {}"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ExactlyOneOfCategory_Succeeds()
    {
        var log = new List<string>();
        var validator = new SystemEnvironmentValidator(new[]
        {
            new FakeComponent("IAS", log, category: "IdentityProvider", required: true),
            new FakeComponent("Entra", log, category: "IdentityProvider", required: true),
        });

        var result = validator.Validate(Env("\"IAS\": {}"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ComponentReportsInvalidSection_Fails()
    {
        var validator = new SystemEnvironmentValidator(new[]
        {
            new FakeComponent("Api", new List<string>(), valid: false),
        });

        var result = validator.Validate(Env("\"Api\": {}"));

        Assert.False(result.IsValid);
    }
}

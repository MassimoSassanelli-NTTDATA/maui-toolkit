using Ndbs.MauiToolkit.Environments;
using Xunit;

namespace Ndbs.MauiToolkit.Tests.Environments;

public sealed class SystemEnvironmentJsonSerializerTests
{
    private const string SampleJson = """
    [
      {
        "Name": "GIIC Products DEV - XIA-DEV",
        "IAS": {
          "ClientId": "d0c6e1da-ca30-4fbd-84e1-3aa281438c05",
          "Authority": "https://itelligence-nonprod.accounts.ondemand.com"
        },
        "Xia": {
          "BaseUrl": "https://gateway.example.com/",
          "Tenant": { "Id": "00000000-0000-0000-0000-000000000000", "Name": "standard" }
        }
      }
    ]
    """;

    [Fact]
    public void Deserialize_ReadsNameAndSections()
    {
        var environments = SystemEnvironmentJsonSerializer.Deserialize(SampleJson);

        var env = Assert.Single(environments);
        Assert.Equal("GIIC Products DEV - XIA-DEV", env.Name);
        Assert.True(env.HasSection("IAS"));
        Assert.True(env.HasSection("Xia"));
    }

    [Fact]
    public void Deserialize_UnknownSectionsArePreserved()
    {
        var json = """[ { "Name": "E1", "Custom": { "Foo": 1 } } ]""";

        var env = SystemEnvironmentJsonSerializer.Deserialize(json)[0];

        Assert.True(env.HasSection("Custom"));
    }

    [Fact]
    public void RoundTrip_PreservesSections()
    {
        var original = SystemEnvironmentJsonSerializer.Deserialize(SampleJson);

        var json = SystemEnvironmentJsonSerializer.Serialize(original);
        var again = SystemEnvironmentJsonSerializer.Deserialize(json);

        var env = Assert.Single(again);
        Assert.Equal("GIIC Products DEV - XIA-DEV", env.Name);
        Assert.True(env.TryGetSection("Xia", out var xia));
        Assert.Equal("https://gateway.example.com/", xia.GetProperty("BaseUrl").GetString());
    }

    [Fact]
    public void Deserialize_NonArray_Throws()
    {
        Assert.Throws<FormatException>(() => SystemEnvironmentJsonSerializer.Deserialize("""{ "Name": "x" }"""));
    }

    [Fact]
    public void Deserialize_MissingName_Throws()
    {
        Assert.Throws<FormatException>(() => SystemEnvironmentJsonSerializer.Deserialize("""[ { "IAS": {} } ]"""));
    }

    [Fact]
    public void Deserialize_InvalidJson_Throws()
    {
        Assert.Throws<FormatException>(() => SystemEnvironmentJsonSerializer.Deserialize("not json"));
    }
}

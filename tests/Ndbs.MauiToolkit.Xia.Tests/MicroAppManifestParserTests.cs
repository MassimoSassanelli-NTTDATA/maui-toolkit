using Ndbs.MauiToolkit.Xia.MicroApps.Manifest;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class MicroAppManifestParserTests
{
    private static string LoadExampleManifest()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "project.json");
        return File.ReadAllText(path);
    }

    [Fact]
    public void Parse_ExamplePackage_ReadsRootMetadata()
    {
        var manifest = MicroAppManifestParser.Parse(LoadExampleManifest());

        Assert.Equal("MSS_ConfirmWarehouseTask", manifest.Name);
        Assert.Equal("MSS_ConfirmWarehouseTask", manifest.DisplayName);
        Assert.Equal("ConfirmTasks", manifest.Description);
    }

    [Fact]
    public void Parse_ExamplePackage_ReadsFormsWithPathsTextsAndEmptyCompletions()
    {
        var manifest = MicroAppManifestParser.Parse(LoadExampleManifest());

        Assert.Equal(4, manifest.Forms.Count);

        var filterForm = manifest.Forms[0];
        Assert.Equal("MSS_ConfirmWarehouseTask", filterForm.Name);
        Assert.Equal("./forms/MSS_ConfirmWarehouseTaskFilter.xaml", filterForm.Path);
        Assert.Empty(filterForm.Completions);
        Assert.Equal(new[] { "./texts/text.csv" }, filterForm.Texts);
    }

    [Fact]
    public void Parse_ExamplePackage_ReadsScalarCustomProperties()
    {
        var manifest = MicroAppManifestParser.Parse(LoadExampleManifest());

        Assert.Equal("Confirm warehouse task", manifest.CustomProperties["title"]);
        Assert.Equal("warehouse", manifest.CustomProperties["category"]);
        Assert.Equal("internal.png", manifest.CustomProperties["icon"]);
    }

    [Fact]
    public void Parse_IgnoresUndeclaredRootAndFormFields()
    {
        // 'formuploadmethod', 'uploadformat', 'publication' and 'adapterarrangements'
        // are undeclared/ignored; parsing must still succeed (§7.4 tolerance).
        var manifest = MicroAppManifestParser.Parse(LoadExampleManifest());

        Assert.Equal("MSS_ConfirmWarehouseTask", manifest.Name);
        Assert.NotEmpty(manifest.Forms);
    }

    [Fact]
    public void Parse_SerializesScalarSettingsAndProperties()
    {
        const string json = """
        {
            "name": "App",
            "customproperties": { "flag": true, "count": 3, "empty": null },
            "forms": [
                {
                    "name": "Form",
                    "path": "./forms/a.xaml",
                    "settings": { "readonly": false, "max": 10 }
                }
            ]
        }
        """;

        var manifest = MicroAppManifestParser.Parse(json);

        Assert.Equal("true", manifest.CustomProperties["flag"]);
        Assert.Equal("3", manifest.CustomProperties["count"]);
        Assert.Null(manifest.CustomProperties["empty"]);

        var settings = manifest.Forms.Single().Settings;
        Assert.Equal("false", settings["readonly"]);
        Assert.Equal("10", settings["max"]);
    }

    [Fact]
    public void Parse_MissingName_Throws()
    {
        const string json = """{ "forms": [] }""";

        Assert.Throws<MicroAppManifestException>(() => MicroAppManifestParser.Parse(json));
    }

    [Fact]
    public void Parse_MissingForms_Throws()
    {
        const string json = """{ "name": "App" }""";

        Assert.Throws<MicroAppManifestException>(() => MicroAppManifestParser.Parse(json));
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        Assert.Throws<MicroAppManifestException>(() => MicroAppManifestParser.Parse("{ not json"));
    }

    [Fact]
    public void Parse_EmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MicroAppManifestParser.Parse("   "));
    }
}

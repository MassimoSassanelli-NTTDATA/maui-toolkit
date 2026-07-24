using NSubstitute;
using Ndbs.MauiToolkit.DynamicTables;
using Ndbs.MauiToolkit.Xia.MicroApps.Manifest;
using Ndbs.MauiToolkit.Xia.MicroApps.Packaging;
using Ndbs.MauiToolkit.Xia.MicroApps.Sync;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class MicroAppContentBuilderTests : IDisposable
{
    private readonly string _root;
    private readonly IDynamicTableImportService _import = Substitute.For<IDynamicTableImportService>();

    public MicroAppContentBuilderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "microapp-content-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);

        _import.ImportCsvAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(new CsvImportResult(
                ci.ArgAt<string>(1) + "_generated", 0, Array.Empty<Ndbs.MauiToolkit.DynamicTables.Record>())));
    }

    private void WriteFile(string relativePath, string content = "a;b\n1;2")
    {
        var full = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
    }

    private static MicroAppServerImage Image(string name) => new(name, name, "desc", "etag-1");

    [Fact]
    public async Task BuildAsync_ImportsOnlyReferencedCsvFiles()
    {
        // Referenced by the manifest.
        WriteFile("texts/text.csv");
        // Present in the package but NOT referenced → must be ignored.
        WriteFile("completions/example.csv");

        var manifest = new MicroAppManifest
        {
            Name = "App",
            Forms = new[]
            {
                new MicroAppManifestForm
                {
                    Name = "Form",
                    Path = "./forms/a.xaml",
                    Texts = new[] { "./texts/text.csv" },
                },
            },
        };

        var builder = new MicroAppContentBuilder(_import);
        var app = await builder.BuildAsync(Image("App"), manifest, _root);

        // Exactly one import (the referenced text file), with the text prefix.
        await _import.Received(1).ImportCsvAsync(
            Arg.Is<string>(p => p.EndsWith("text.csv", StringComparison.Ordinal)),
            MicroAppTablePrefixes.Text,
            Arg.Any<CancellationToken>());
        await _import.DidNotReceive().ImportCsvAsync(
            Arg.Is<string>(p => p.Contains("example.csv", StringComparison.Ordinal)),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        var textFile = Assert.Single(app.Formulare.Single().Texte);
        Assert.Equal("text_generated", textFile.Inhaltstabelle);
        Assert.Empty(app.Formulare.Single().Vervollstaendigungen);
    }

    [Fact]
    public async Task BuildAsync_ReferencedButMissingFile_CreatesNoContentTable()
    {
        var manifest = new MicroAppManifest
        {
            Name = "App",
            Forms = new[]
            {
                new MicroAppManifestForm
                {
                    Name = "Form",
                    Path = "./forms/a.xaml",
                    Completions = new[] { "./completions/missing.csv" },
                },
            },
        };

        var builder = new MicroAppContentBuilder(_import);
        var app = await builder.BuildAsync(Image("App"), manifest, _root);

        await _import.DidNotReceive().ImportCsvAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        var completion = Assert.Single(app.Formulare.Single().Vervollstaendigungen);
        Assert.Null(completion.Inhaltstabelle);
        Assert.Equal("missing.csv", completion.Dateiname);
    }

    [Fact]
    public async Task BuildAsync_UsesCompletionPrefixForCompletionFiles()
    {
        WriteFile("completions/data.csv");

        var manifest = new MicroAppManifest
        {
            Name = "App",
            Forms = new[]
            {
                new MicroAppManifestForm
                {
                    Name = "Form",
                    Path = "./forms/a.xaml",
                    Completions = new[] { "./completions/data.csv" },
                },
            },
        };

        var builder = new MicroAppContentBuilder(_import);
        await builder.BuildAsync(Image("App"), manifest, _root);

        await _import.Received(1).ImportCsvAsync(
            Arg.Any<string>(), MicroAppTablePrefixes.Completion, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuildAsync_MapsMasterDataAndCustomProperties()
    {
        var manifest = new MicroAppManifest
        {
            Name = "App",
            DisplayName = "Manifest Display",
            Description = "Manifest Desc",
            CustomProperties = new Dictionary<string, string?> { ["title"] = "T", ["category"] = "C" },
            Forms = Array.Empty<MicroAppManifestForm>(),
        };

        var builder = new MicroAppContentBuilder(_import);
        var app = await builder.BuildAsync(Image("App"), manifest, _root);

        Assert.Equal("App", app.Name);
        Assert.Equal("Manifest Display", app.Anzeigename);
        Assert.Equal("Manifest Desc", app.Beschreibung);
        Assert.Equal("etag-1", app.ETag);
        Assert.True(app.IstSynchronisiert);
        Assert.Equal(2, app.Eigenschaften.Count);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}

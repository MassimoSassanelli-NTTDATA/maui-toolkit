using NDBS.Xia.Api.Dtos;
using Ndbs.MauiToolkit.Xia.MicroApps.Sync;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class MicroAppServerImageBuilderTests
{
    private static ProjectResult Meta(string name, string display, string description)
    {
        var project = new ProjectResult { Name = name };
        project.Modes.Add(new ModeResult
        {
            ProjectInfo = new ProjectInfo { DisplayName = display, Description = description },
        });
        return project;
    }

    private static ProjectMetadataFile Head(string name, string etag) => new()
    {
        Etag = etag,
        Identifier = new ProjectIdentifier { Name = name },
    };

    [Fact]
    public void Build_MergesMetaAndHeadOverName()
    {
        var meta = new[] { Meta("A", "App A", "First"), Meta("B", "App B", "Second") };
        var head = new[] { Head("A", "etag-a"), Head("B", "etag-b") };

        var images = MicroAppServerImageBuilder.Build(meta, head)
            .ToDictionary(i => i.Name);

        Assert.Equal(2, images.Count);
        Assert.Equal("App A", images["A"].DisplayName);
        Assert.Equal("First", images["A"].Description);
        Assert.Equal("etag-a", images["A"].ETag);
        Assert.Equal("etag-b", images["B"].ETag);
    }

    [Fact]
    public void Build_IncludesMicroAppsPresentInOnlyOneResult()
    {
        var meta = new[] { Meta("OnlyMeta", "Only Meta", "desc") };
        var head = new[] { Head("OnlyHead", "etag-h") };

        var images = MicroAppServerImageBuilder.Build(meta, head)
            .ToDictionary(i => i.Name);

        Assert.Equal(2, images.Count);

        Assert.Equal("Only Meta", images["OnlyMeta"].DisplayName);
        Assert.Null(images["OnlyMeta"].ETag);

        Assert.Null(images["OnlyHead"].DisplayName);
        Assert.Equal("etag-h", images["OnlyHead"].ETag);
    }

    [Fact]
    public void Build_WithNullInputs_ReturnsEmpty()
    {
        var images = MicroAppServerImageBuilder.Build(null, null);

        Assert.Empty(images);
    }

    [Fact]
    public void Build_SkipsEntriesWithoutName()
    {
        var meta = new[] { Meta("A", "App A", "desc"), new ProjectResult { Name = "" } };

        var images = MicroAppServerImageBuilder.Build(meta, null);

        Assert.Single(images);
        Assert.Equal("A", images[0].Name);
    }
}

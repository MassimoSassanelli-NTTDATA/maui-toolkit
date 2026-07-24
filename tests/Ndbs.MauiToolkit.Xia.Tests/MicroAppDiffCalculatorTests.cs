using Ndbs.MauiToolkit.Xia.MicroApps.Sync;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class MicroAppDiffCalculatorTests
{
    private static MicroAppServerImage Server(string name, string? etag)
        => new(name, name, null, etag);

    private static MicroAppLocalState Local(string name, string? etag, bool synced = true)
        => new(name, etag, synced);

    [Fact]
    public void Compute_ClassifiesAllFiveCategories()
    {
        var server = new[]
        {
            Server("Added", "e1"),
            Server("Changed", "e2-new"),
            Server("Unchanged", "e3"),
            Server("Faulted", "e4"),
        };

        var local = new[]
        {
            Local("Changed", "e2-old"),
            Local("Unchanged", "e3"),
            Local("Faulted", "e4", synced: false),
            Local("Removed", "e5"),
        };

        var diff = MicroAppDiffCalculator.Compute(server, local);

        Assert.Equal(new[] { "Added" }, diff.Added.Select(i => i.Name));
        Assert.Equal(new[] { "Changed" }, diff.Changed.Select(i => i.Name));
        Assert.Equal(new[] { "Unchanged" }, diff.Unchanged.Select(i => i.Name));
        Assert.Equal(new[] { "Faulted" }, diff.Faulted.Select(i => i.Name));
        Assert.Equal(new[] { "Removed" }, diff.Removed);
    }

    [Fact]
    public void Compute_FoldsFaultedIntoToUpdate()
    {
        var server = new[] { Server("Changed", "new"), Server("Faulted", "e") };
        var local = new[]
        {
            Local("Changed", "old"),
            Local("Faulted", "e", synced: false),
        };

        var diff = MicroAppDiffCalculator.Compute(server, local);

        Assert.Equal(new[] { "Changed", "Faulted" }, diff.ToUpdate.Select(i => i.Name));
        Assert.True(diff.HasChanges);
    }

    [Fact]
    public void Compute_NoDifferences_HasNoChanges()
    {
        var server = new[] { Server("A", "e1"), Server("B", "e2") };
        var local = new[] { Local("A", "e1"), Local("B", "e2") };

        var diff = MicroAppDiffCalculator.Compute(server, local);

        Assert.False(diff.HasChanges);
        Assert.Empty(diff.Added);
        Assert.Empty(diff.Removed);
        Assert.Empty(diff.ToUpdate);
        Assert.Equal(2, diff.Unchanged.Count);
    }

    [Fact]
    public void Compute_SameEtagButFaulted_GoesToUpdateNotUnchanged()
    {
        var server = new[] { Server("A", "e1") };
        var local = new[] { Local("A", "e1", synced: false) };

        var diff = MicroAppDiffCalculator.Compute(server, local);

        Assert.Empty(diff.Unchanged);
        Assert.Equal(new[] { "A" }, diff.Faulted.Select(i => i.Name));
        Assert.Equal(new[] { "A" }, diff.ToUpdate.Select(i => i.Name));
    }
}

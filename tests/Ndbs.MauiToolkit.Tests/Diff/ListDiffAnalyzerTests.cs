using Ndbs.MauiToolkit.Diff;
using Xunit;

namespace Ndbs.MauiToolkit.Tests.Diff
{
    public class ListDiffAnalyzerTests
    {
        private sealed record Record(string? Id, int Version);

        private static ListDiffAnalyzer<Record> CreateAnalyzer()
            => new(record => record.Id!, (local, remote) => local.Version != remote.Version);

        [Fact]
        public void Constructor_NullIdSelector_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ListDiffAnalyzer<Record>(null!, (l, r) => false));
        }

        [Fact]
        public void Constructor_NullIsChangedFunc_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ListDiffAnalyzer<Record>(record => record.Id!, null!));
        }

        [Fact]
        public void Calculate_NullRemoteList_Throws()
        {
            var analyzer = CreateAnalyzer();

            Assert.Throws<ArgumentNullException>(
                () => analyzer.Calculate(null!, Array.Empty<Record>()));
        }

        [Fact]
        public void Calculate_NullLocalList_Throws()
        {
            var analyzer = CreateAnalyzer();

            Assert.Throws<ArgumentNullException>(
                () => analyzer.Calculate(Array.Empty<Record>(), null!));
        }

        [Fact]
        public void Calculate_ItemOnlyInRemote_ReturnsAdded()
        {
            var analyzer = CreateAnalyzer();
            var remote = new[] { new Record("1", 1) };
            var local = Array.Empty<Record>();

            var diff = analyzer.Calculate(remote, local);

            var entry = Assert.Single(diff);
            Assert.Equal(DiffTypes.Added, entry.Type);
            Assert.Null(entry.Local);
            Assert.Equal(remote[0], entry.Remote);
        }

        [Fact]
        public void Calculate_ItemOnlyInLocal_ReturnsRemoved()
        {
            var analyzer = CreateAnalyzer();
            var remote = Array.Empty<Record>();
            var local = new[] { new Record("1", 1) };

            var diff = analyzer.Calculate(remote, local);

            var entry = Assert.Single(diff);
            Assert.Equal(DiffTypes.Removed, entry.Type);
            Assert.Equal(local[0], entry.Local);
            Assert.Null(entry.Remote);
        }

        [Fact]
        public void Calculate_ItemInBothWithDifferentVersion_ReturnsChanged()
        {
            var analyzer = CreateAnalyzer();
            var remote = new[] { new Record("1", 2) };
            var local = new[] { new Record("1", 1) };

            var diff = analyzer.Calculate(remote, local);

            var entry = Assert.Single(diff);
            Assert.Equal(DiffTypes.Changed, entry.Type);
            Assert.Equal(local[0], entry.Local);
            Assert.Equal(remote[0], entry.Remote);
        }

        [Fact]
        public void Calculate_ItemInBothWithSameVersion_ReturnsUnchanged()
        {
            var analyzer = CreateAnalyzer();
            var remote = new[] { new Record("1", 1) };
            var local = new[] { new Record("1", 1) };

            var diff = analyzer.Calculate(remote, local);

            var entry = Assert.Single(diff);
            Assert.Equal(DiffTypes.Unchanged, entry.Type);
            Assert.Equal(local[0], entry.Local);
            Assert.Equal(remote[0], entry.Remote);
        }

        [Fact]
        public void Calculate_BothListsEmpty_ReturnsEmpty()
        {
            var analyzer = CreateAnalyzer();

            var diff = analyzer.Calculate(Array.Empty<Record>(), Array.Empty<Record>());

            Assert.Empty(diff);
        }

        [Fact]
        public void Calculate_MixedScenarios_ClassifiesEachItem()
        {
            var analyzer = CreateAnalyzer();
            var remote = new[]
            {
                new Record("added", 1),
                new Record("changed", 2),
                new Record("unchanged", 5),
            };
            var local = new[]
            {
                new Record("removed", 1),
                new Record("changed", 1),
                new Record("unchanged", 5),
            };

            var diff = analyzer.Calculate(remote, local);

            Assert.Equal(DiffTypes.Added, Assert.Single(diff, d => d.Remote?.Id == "added").Type);
            Assert.Equal(DiffTypes.Removed, Assert.Single(diff, d => d.Local?.Id == "removed").Type);
            Assert.Equal(DiffTypes.Changed, Assert.Single(diff, d => d.Local?.Id == "changed").Type);
            Assert.Equal(DiffTypes.Unchanged, Assert.Single(diff, d => d.Local?.Id == "unchanged").Type);
        }

        [Fact]
        public void Calculate_DuplicateIdsInLocalList_ThrowsDescriptiveException()
        {
            var analyzer = CreateAnalyzer();
            var remote = Array.Empty<Record>();
            var local = new[] { new Record("1", 1), new Record("1", 2) };

            var exception = Assert.Throws<InvalidOperationException>(
                () => analyzer.Calculate(remote, local));

            Assert.Contains("duplicate identifier", exception.Message);
            Assert.Contains("localList", exception.Message);
            Assert.Contains("'1'", exception.Message);
        }

        [Fact]
        public void Calculate_DuplicateIdsInRemoteList_ThrowsDescriptiveException()
        {
            var analyzer = CreateAnalyzer();
            var remote = new[] { new Record("1", 1), new Record("1", 2) };
            var local = Array.Empty<Record>();

            var exception = Assert.Throws<InvalidOperationException>(
                () => analyzer.Calculate(remote, local));

            Assert.Contains("duplicate identifier", exception.Message);
            Assert.Contains("remoteList", exception.Message);
        }

        [Fact]
        public void Calculate_NullIdInLocalList_ThrowsDescriptiveException()
        {
            var analyzer = CreateAnalyzer();
            var remote = Array.Empty<Record>();
            var local = new[] { new Record(null, 1) };

            var exception = Assert.Throws<InvalidOperationException>(
                () => analyzer.Calculate(remote, local));

            Assert.Contains("null identifier", exception.Message);
            Assert.Contains("localList", exception.Message);
        }

        [Fact]
        public void Calculate_MultipleNullIds_DoNotSilentlyCollapse()
        {
            var analyzer = CreateAnalyzer();
            var remote = Array.Empty<Record>();
            var local = new[] { new Record(null, 1), new Record(null, 2) };

            // Without explicit handling these would collapse onto a single empty-string key.
            Assert.Throws<InvalidOperationException>(() => analyzer.Calculate(remote, local));
        }

        [Fact]
        public void Calculate_CancelledToken_ThrowsOperationCanceled()
        {
            var analyzer = CreateAnalyzer();
            var remote = new[] { new Record("1", 1) };
            var local = new[] { new Record("2", 1) };
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.Throws<OperationCanceledException>(
                () => analyzer.Calculate(remote, local, cts.Token));
        }
    }
}
